using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    [Authorize(Roles = "Admin,Financeiro")]
    public class FinanceiroController : Controller
    {
        private readonly AppDbContext _context;

        public FinanceiroController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard Financeiro e Livro Caixa
        public IActionResult Index()
        {
            var transacoes = _context.Financeiro.OrderByDescending(t => t.DataTransacao).ToList();
            
            ViewBag.SaldoTotal = transacoes.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor) - 
                                 transacoes.Where(t => t.Tipo == "Saída").Sum(t => t.Valor);
            ViewBag.PendenteNfe = transacoes.Count(t => t.ExigeNotaFiscal && t.StatusSefaz == "Pendente");

            return View(transacoes);
        }

        [HttpGet]
        public IActionResult NovaTransacao()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NovaTransacao(TransacaoFinanceira transacao)
        {
            if (transacao.Descricao?.Length > 160)
            {
                ModelState.AddModelError(nameof(transacao.Descricao), "A descrição deve ter até 160 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                return View(transacao);
            }

            transacao.DataTransacao = DateTime.Now;
            
            // Lógica inicial para SEFAZ (Futuramente ligada via API)
            if (transacao.ExigeNotaFiscal)
            {
                transacao.StatusSefaz = "Pendente";
            }

            _context.Financeiro.Add(transacao);
            _context.SaveChanges();

            TempData["MensagemSucesso"] = "Registo adicionado ao Livro Caixa com sucesso!";
            return RedirectToAction("Index");
        }

        // Bloqueado até existir validação de assinatura fornecida pelo banco.
        [HttpPost]
        [AllowAnonymous]
        public IActionResult WebhookPixRecebido()
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "Webhook indisponível até que a assinatura do provedor bancário seja configurada.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SimularEmissaoNfe(int id)
        {
            var transacao = _context.Financeiro.Find(id);
            
            if (transacao != null && transacao.ExigeNotaFiscal && transacao.StatusSefaz == "Pendente")
            {
                transacao.StatusSefaz = "Simulada";
                transacao.ChaveAcessoNFe = null;
                
                _context.Financeiro.Update(transacao);
                _context.SaveChanges();

                TempData["MensagemSucesso"] = "Simulação registrada. Nenhuma NF-e foi emitida pela SEFAZ.";
            }
            else
            {
                TempData["Erro"] = "Transação não encontrada ou já emitida.";
            }

            return RedirectToAction("Index");
        }
    } // <-- Fim da classe FinanceiroController

} // <-- Fim do namespace