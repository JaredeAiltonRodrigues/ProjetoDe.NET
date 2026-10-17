using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OngDoacao.Data;
using OngDoacao.Models;
using OngDoacao.Servicos;
using Microsoft.AspNetCore.Authorization;   

namespace OngDoacao.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NotificacaoService _notificacaoService;
        private readonly IaService _iaService;

        public AdminController(AppDbContext context, NotificacaoService notificacaoService, IaService iaService)
        {
            _context = context;
            _notificacaoService = notificacaoService;
            _iaService = iaService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var agora = DateTime.Now;
            var alimentos = await _context.Alimentos
                .Where(a => a.DataVencimento > agora && a.Quantidade > 0)
                .ToListAsync();
            var totalItens = alimentos.Sum(a => a.Quantidade);
            var fila = await _context.Beneficiarios
                .OrderBy(b => b.DataCadastro)
                .ThenBy(b => b.Id)
                .ToListAsync();
            var aguardando = fila.Count(b => b.StatusFila == "Aguardando");
            var cestasDisponiveis = totalItens / 5;
            var beneficiariosPrevistos = fila
                .Where(b => b.StatusFila == "Aguardando")
                .Take(cestasDisponiveis)
                .ToList();

            // Comente a chamada da IA para não travar a tela
            // var conselho = await _iaService.ObterConselhoDoGestorAsync(totalItens, cestasDisponiveis, aguardando);
    
                // Coloque um texto fixo temporário
             var conselho = "A integração com a IA está temporariamente desligada para testes.";

             var model = new DashboardViewModel
                {
                    TotalItensEstoque = totalItens,
                    CestasDisponiveis = cestasDisponiveis,
                    Fila = fila,
                    SugestaoIA = conselho 
                 };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarCestas()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var agora = DateTime.Now;
            var alimentos = await _context.Alimentos
                .Where(a => a.DataVencimento > agora && a.Quantidade > 0)
                .OrderBy(a => a.DataVencimento)
                .ThenBy(a => a.Id)
                .ToListAsync();
            var totalItens = alimentos.Sum(a => a.Quantidade);
            var pessoasAtendidas = await _context.Beneficiarios
                .Where(b => b.StatusFila == "Aguardando")
                .OrderBy(b => b.DataCadastro)
                .ThenBy(b => b.Id)
                .Take(totalItens / 5)
                .ToListAsync();

            if (pessoasAtendidas.Count == 0)
            {
                TempData["Erro"] = totalItens < 5
                    ? "Não há estoque suficiente para montar uma cesta."
                    : "Não há beneficiários aguardando na fila.";
                return RedirectToAction("Dashboard");
            }

            DateTime horarioAgendamento = DateTime.Today.AddDays(1).AddHours(8);

            foreach (var pessoa in pessoasAtendidas)
            {
                pessoa.StatusFila = "Cesta Reservada";
                pessoa.HorarioRetirada = horarioAgendamento;
                horarioAgendamento = horarioAgendamento.AddMinutes(15);
            }

            var itensARemover = pessoasAtendidas.Count * 5;
            foreach (var alimento in alimentos)
            {
                var quantidadeUsada = Math.Min(alimento.Quantidade, itensARemover);
                alimento.Quantidade -= quantidadeUsada;
                itensARemover -= quantidadeUsada;

                if (alimento.Quantidade == 0)
                {
                    _context.Alimentos.Remove(alimento);
                }

                if (itensARemover == 0)
                {
                    break;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var pessoa in pessoasAtendidas)
            {
                await _notificacaoService.AvisarBeneficiarioAsync(pessoa);
            }

            TempData["MensagemSucesso"] = $"{pessoasAtendidas.Count} cestas foram liberadas com sucesso e os beneficiários foram notificados!";
            return RedirectToAction("Dashboard");
        }
    }
}
        