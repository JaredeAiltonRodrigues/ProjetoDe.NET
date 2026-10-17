using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    // Apenas Administradores e pessoal do Estoque podem entrar
    [Authorize(Roles = "Admin,Estoque")]
    public class AlimentosController : Controller
    {
        private readonly AppDbContext _context;

        public AlimentosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TELA PRINCIPAL: Lista tudo o que tem no armazém
        [HttpGet]
        public IActionResult Index()
        {
            // Traz os alimentos ordenados pela data de validade (os que vencem primeiro aparecem no topo)
            var estoque = _context.Alimentos.OrderBy(a => a.DataVencimento).ToList();
            return View(estoque);
        }

        // 2. TELA DA CÂMARA: Continua a funcionar para dar entrada
        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Alimento alimento)
        {
            _context.Alimentos.Add(alimento);
            _context.SaveChanges();

            TempData["MensagemSucesso"] = $"Entrada do alimento '{alimento.Nome}' registada com sucesso!";
            // Agora, ao ler com a câmara, ele volta para a lista organizada em vez da página inicial
            return RedirectToAction("Index"); 
        }

        // 3. NOVA AÇÃO: Dar baixa num item (ex: se estragou ou foi entregue avulso)
        [HttpPost]
        public IActionResult Remover(int id)
        {
            var item = _context.Alimentos.Find(id);
            if (item != null)
            {
                _context.Alimentos.Remove(item);
                _context.SaveChanges();
                TempData["MensagemSucesso"] = $"O item {item.Nome} foi removido do estoque.";
            }
            return RedirectToAction("Index");
        }
    }
}