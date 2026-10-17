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
        [ValidateAntiForgeryToken]
        public IActionResult Cadastro(
            [Bind(nameof(Beneficiario.Nome), nameof(Beneficiario.Cpf), nameof(Beneficiario.TamanhoFamilia), nameof(Beneficiario.Telefone), nameof(Beneficiario.Email))]
            Beneficiario beneficiario,
            bool aceitePrivacidade)
        {
            if (string.IsNullOrWhiteSpace(beneficiario.Nome) || beneficiario.Nome.Length > 120)
            {
                ModelState.AddModelError(nameof(beneficiario.Nome), "Informe um nome com até 120 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(beneficiario.Cpf))
            {
                ModelState.AddModelError(nameof(beneficiario.Cpf), "Informe o CPF.");
            }

            if (string.IsNullOrWhiteSpace(beneficiario.Telefone) || beneficiario.Telefone.Length > 30)
            {
                ModelState.AddModelError(nameof(beneficiario.Telefone), "Informe um telefone com até 30 caracteres.");
            }

            if (beneficiario.Email?.Length > 254)
            {
                ModelState.AddModelError(nameof(beneficiario.Email), "O e-mail deve ter até 254 caracteres.");
            }

            if (!aceitePrivacidade)
            {
                ModelState.AddModelError(string.Empty, "É necessário aceitar o aviso de privacidade para enviar o cadastro.");
            }

            if (!ModelState.IsValid)
            {
                return View(beneficiario);
            }

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
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatus(int id, string novoStatus)
        {
            var pessoa = _context.Beneficiarios.Find(id);
            if (pessoa == null)
            {
                return NotFound();
            }

            if (pessoa.StatusFila != "Cesta Reservada" || novoStatus != "Cesta Entregue")
            {
                return BadRequest();
            }

            pessoa.StatusFila = novoStatus;
            var distribuicao = _context.Distribuicoes
                .Where(item => item.BeneficiarioId == pessoa.Id && item.Status == "Reservada")
                .OrderByDescending(item => item.DataReserva)
                .FirstOrDefault();
            if (distribuicao != null)
            {
                distribuicao.Status = "Entregue";
                distribuicao.DataEntrega = DateTime.Now;
            }
            _context.SaveChanges();
            TempData["MensagemSucesso"] = $"O status de {pessoa.Nome} foi atualizado para '{novoStatus}'.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remover(int id)
        {
            var pessoa = _context.Beneficiarios.Find(id);
            if (pessoa != null)
            {
                if (_context.Distribuicoes.Any(item => item.BeneficiarioId == id))
                {
                    TempData["Erro"] = "Este cadastro possui histórico de distribuição e não pode ser removido.";
                    return RedirectToAction("Index");
                }

                _context.Beneficiarios.Remove(pessoa);
                _context.SaveChanges();
                TempData["MensagemSucesso"] = "O registo foi removido do sistema com sucesso.";
            }
            return RedirectToAction("Index");
        }
    }
}