using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    public class BeneficiariosController : Controller
    {
        private readonly AppDbContext _context;

        public BeneficiariosController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ÁREA PÚBLICA (Qualquer pessoa pode aceder)
        // ==========================================
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Cadastro(Beneficiario beneficiario)
        {
            beneficiario.DataCadastro = DateTime.Now;
            beneficiario.StatusFila = "Aguardando";
            
            _context.Beneficiarios.Add(beneficiario);
            _context.SaveChanges();

            TempData["MensagemSucesso"] = "Cadastro realizado com sucesso! Está agora na fila de espera.";
            return RedirectToAction("Index", "Home");
        }


        // ==========================================
        // ÁREA RESTRITA (Apenas Administradores)
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index()
        {
            // Lista todos os beneficiários, agrupando primeiro pelo status e depois pela data mais antiga
            var fila = _context.Beneficiarios
                .OrderBy(b => b.StatusFila == "Cesta Entregue" ? 1 : 0) // Coloca os já entregues no fim da lista
                .ThenBy(b => b.DataCadastro)
                .ToList();
                
            return View(fila);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AtualizarStatus(int id, string novoStatus)
        {
            var pessoa = _context.Beneficiarios.Find(id);
            if (pessoa != null)
            {
                pessoa.StatusFila = novoStatus;
                _context.Beneficiarios.Update(pessoa);
                _context.SaveChanges();
                TempData["MensagemSucesso"] = $"O status de {pessoa.Nome} foi atualizado para '{novoStatus}'.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Remover(int id)
        {
            var pessoa = _context.Beneficiarios.Find(id);
            if (pessoa != null)
            {
                _context.Beneficiarios.Remove(pessoa);
                _context.SaveChanges();
                TempData["MensagemSucesso"] = "O registo foi removido do sistema com sucesso.";
            }
            return RedirectToAction("Index");
        }
    }
}