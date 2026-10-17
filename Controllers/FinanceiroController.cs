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
            ViewBag.PendenteNfe = transacoes.Count(t => t.ExigeNotaFiscal && t.StatusSefaz != "Emitida");

            return View(transacoes);
        }

        [HttpGet]
        public IActionResult NovaTransacao()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NovaTransacao(TransacaoFinanceira transacao)
        {
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

        // Endpoint simulado para receber Webhooks do Banco (Ex: PIX recebido)
        [HttpPost]
        [AllowAnonymous] // Permite que a API do banco aceda a este endpoint
        public IActionResult WebhookPixRecebido([FromBody] TransacaoPixDto payload)
        {
            var novaDoacao = new TransacaoFinanceira
            {
                Descricao = $"Doação PIX - {payload.Mensagem}",
                Valor = payload.Valor,
                Tipo = "Entrada",
                MetodoPagamento = "PIX",
                IdentificacaoPagador = payload.NomePagador,
                IdTransacaoBancaria = payload.TxId,
                Conciliado = true,
                ExigeNotaFiscal = true,
                StatusSefaz = "Pendente" 
            };

            _context.Financeiro.Add(novaDoacao);
            _context.SaveChanges();

            return Ok();
        }

        // SIMULADOR DE SEFAZ PARA TESTES
        [HttpPost]
        public IActionResult SimularEmissaoNfe(int id)
        {
            var transacao = _context.Financeiro.Find(id);
            
            if (transacao != null && transacao.ExigeNotaFiscal && transacao.StatusSefaz == "Pendente")
            {
                // Finge que enviou para a API da Receita Federal e demorou 1 segundo
                System.Threading.Thread.Sleep(1000); 

                // Gera uma Chave de Acesso Falsa de 44 dígitos para simular a SEFAZ
                Random random = new Random();
                string chaveFalsa = "";
                for (int i = 0; i < 44; i++) {
                    chaveFalsa += random.Next(0, 10).ToString();
                }

                // Atualiza o banco de dados
                transacao.StatusSefaz = "Emitida";
                transacao.ChaveAcessoNFe = chaveFalsa;
                
                _context.Financeiro.Update(transacao);
                _context.SaveChanges();

                TempData["MensagemSucesso"] = $"Nota Fiscal Simulada com Sucesso! Chave: {chaveFalsa}";
            }
            else
            {
                TempData["Erro"] = "Transação não encontrada ou já emitida.";
            }

            return RedirectToAction("Index");
        }
    } // <-- Fim da classe FinanceiroController

    // DTO para mapear os dados que o Banco envia via API
    public class TransacaoPixDto
    {
        public decimal Valor { get; set; }
        public string? NomePagador { get; set; }
        public string? TxId { get; set; }
        public string? Mensagem { get; set; }
    }
} // <-- Fim do namespace