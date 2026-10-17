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
            var hoje = DateTime.Today;
            var alimentos = await _context.Alimentos
                .Where(a => a.DataVencimento >= hoje && a.Quantidade > 0)
                .ToListAsync();
            var totalItens = alimentos.Sum(a => a.Quantidade);
            var proximosVencimentos = await _context.Alimentos
                .Where(a => a.Quantidade > 0 && a.DataVencimento >= hoje && a.DataVencimento <= hoje.AddDays(7))
                .OrderBy(a => a.DataVencimento)
                .Take(8)
                .ToListAsync();
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

            var conselho = await _iaService.ObterConselhoDoGestorAsync(
                totalItens, cestasDisponiveis, aguardando, beneficiariosPrevistos.Count);
            var assistenteDisponivel = !conselho.StartsWith("Assistente indisponível", StringComparison.OrdinalIgnoreCase);
            var distribuicoesRecentes = await _context.Distribuicoes
                .Include(distribuicao => distribuicao.Beneficiario)
                .Include(distribuicao => distribuicao.Itens)
                .OrderByDescending(distribuicao => distribuicao.DataReserva)
                .Take(12)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalItensEstoque = totalItens,
                CestasDisponiveis = cestasDisponiveis,
                ItensVencendoEmSeteDias = proximosVencimentos.Sum(alimento => alimento.Quantidade),
                ReservasAtivas = await _context.Distribuicoes.CountAsync(distribuicao => distribuicao.Status == "Reservada"),
                EntregasConcluidas = await _context.Distribuicoes.CountAsync(distribuicao => distribuicao.Status == "Entregue"),
                NotificacoesPendentes = await _context.Distribuicoes.CountAsync(distribuicao =>
                    distribuicao.Status == "Reservada" &&
                    distribuicao.StatusNotificacao != "Enviada" &&
                    distribuicao.StatusNotificacao != "Contato manual"),
                Fila = fila,
                BeneficiariosPrevistos = beneficiariosPrevistos,
                DistribuicoesRecentes = distribuicoesRecentes,
                ProximosVencimentos = proximosVencimentos,
                SugestaoIA = conselho,
                AssistenteDisponivel = assistenteDisponivel
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Doacoes()
        {
            var distribuicoes = await _context.Distribuicoes
                .Include(distribuicao => distribuicao.Beneficiario)
                .Include(distribuicao => distribuicao.Itens)
                .OrderByDescending(distribuicao => distribuicao.DataReserva)
                .ToListAsync();

            return View(distribuicoes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarCestas()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var hoje = DateTime.Today;
            var alimentos = await _context.Alimentos
                .Where(a => a.DataVencimento >= hoje && a.Quantidade > 0)
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

            var distribuicoes = new List<DistribuicaoDoacao>();
            DateTime horarioAgendamento = DateTime.Today.AddDays(1).AddHours(8);
            foreach (var pessoa in pessoasAtendidas)
            {
                var distribuicao = new DistribuicaoDoacao
                {
                    BeneficiarioId = pessoa.Id,
                    Beneficiario = pessoa,
                    DataReserva = DateTime.Now,
                    HorarioRetirada = horarioAgendamento,
                    Status = "Reservada",
                    StatusNotificacao = "Pendente"
                };

                var itensRestantes = 5;
                foreach (var alimento in alimentos.Where(alimento => alimento.Quantidade > 0))
                {
                    var quantidadeSeparada = Math.Min(alimento.Quantidade, itensRestantes);
                    if (quantidadeSeparada == 0)
                    {
                        continue;
                    }

                    distribuicao.Itens.Add(new DistribuicaoDoacaoItem
                    {
                        NomeAlimento = alimento.Nome ?? "Alimento sem nome",
                        CodigoDeBarras = alimento.CodigoDeBarras,
                        DataVencimento = alimento.DataVencimento,
                        Quantidade = quantidadeSeparada
                    });
                    alimento.Quantidade -= quantidadeSeparada;
                    itensRestantes -= quantidadeSeparada;

                    if (itensRestantes == 0)
                    {
                        break;
                    }
                }

                if (itensRestantes != 0)
                {
                    throw new InvalidOperationException("O estoque mudou durante a reserva. Nenhuma distribuição foi concluída.");
                }

                pessoa.StatusFila = "Cesta Reservada";
                pessoa.HorarioRetirada = horarioAgendamento;
                distribuicoes.Add(distribuicao);
                horarioAgendamento = horarioAgendamento.AddMinutes(15);
            }

            foreach (var alimento in alimentos)
            {
                if (alimento.Quantidade == 0)
                {
                    _context.Alimentos.Remove(alimento);
                }
            }

            _context.Distribuicoes.AddRange(distribuicoes);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var distribuicao in distribuicoes)
            {
                var resultado = await _notificacaoService.AvisarBeneficiarioAsync(distribuicao.Beneficiario);
                distribuicao.StatusNotificacao = resultado.Status;
                distribuicao.DetalheNotificacao = resultado.Mensagem;
                distribuicao.DataNotificacao = resultado.Status == "Enviada" ? DateTime.Now : null;
            }
            await _context.SaveChangesAsync();

            var notificacoesEnviadas = distribuicoes.Count(distribuicao => distribuicao.StatusNotificacao == "Enviada");
            TempData["MensagemSucesso"] = $"{distribuicoes.Count} cestas reservadas com os itens registrados. Notificações enviadas: {notificacoesEnviadas} de {distribuicoes.Count}.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarNotificacao(int id)
        {
            var distribuicao = await _context.Distribuicoes
                .Include(item => item.Beneficiario)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (distribuicao == null)
            {
                return NotFound();
            }

            if (distribuicao.Status != "Reservada")
            {
                TempData["Erro"] = "Só é possível notificar uma cesta ainda reservada.";
                return RedirectToAction("Doacoes");
            }

            var resultado = await _notificacaoService.AvisarBeneficiarioAsync(distribuicao.Beneficiario);
            distribuicao.StatusNotificacao = resultado.Status;
            distribuicao.DetalheNotificacao = resultado.Mensagem;
            distribuicao.DataNotificacao = resultado.Status == "Enviada" ? DateTime.Now : null;
            await _context.SaveChangesAsync();

            TempData[resultado.Status == "Enviada" ? "MensagemSucesso" : "Erro"] = resultado.Mensagem;
            return RedirectToAction("Doacoes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarContatoManual(int id)
        {
            var distribuicao = await _context.Distribuicoes
                .Include(item => item.Beneficiario)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (distribuicao == null)
            {
                return NotFound();
            }

            if (distribuicao.Status != "Reservada" || string.IsNullOrWhiteSpace(distribuicao.Beneficiario.Telefone))
            {
                TempData["Erro"] = "Não há telefone válido ou reserva ativa para registrar o contato.";
                return RedirectToAction("Doacoes");
            }

            distribuicao.StatusNotificacao = "Contato manual";
            distribuicao.DataNotificacao = DateTime.Now;
            distribuicao.DetalheNotificacao = "A equipe registrou que entrou em contato por telefone.";
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Contato telefônico registrado no histórico.";
            return RedirectToAction("Doacoes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEntrega(int id)
        {
            var distribuicao = await _context.Distribuicoes
                .Include(item => item.Beneficiario)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (distribuicao == null)
            {
                return NotFound();
            }

            if (distribuicao.Status != "Reservada")
            {
                TempData["Erro"] = "Esta distribuição não está mais reservada.";
                return RedirectToAction("Doacoes");
            }

            distribuicao.Status = "Entregue";
            distribuicao.DataEntrega = DateTime.Now;
            distribuicao.Beneficiario.StatusFila = "Cesta Entregue";
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Entrega registrada no histórico.";
            return RedirectToAction("Doacoes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarDistribuicao(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var distribuicao = await _context.Distribuicoes
                .Include(item => item.Beneficiario)
                .Include(item => item.Itens)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (distribuicao == null)
            {
                return NotFound();
            }

            if (distribuicao.Status != "Reservada")
            {
                TempData["Erro"] = "Somente reservas ainda não entregues podem ser canceladas.";
                return RedirectToAction("Doacoes");
            }

            foreach (var item in distribuicao.Itens)
            {
                var alimento = await _context.Alimentos.FirstOrDefaultAsync(candidato =>
                    candidato.Nome == item.NomeAlimento &&
                    candidato.CodigoDeBarras == item.CodigoDeBarras &&
                    candidato.DataVencimento == item.DataVencimento);

                if (alimento == null)
                {
                    _context.Alimentos.Add(new Alimento
                    {
                        Nome = item.NomeAlimento,
                        CodigoDeBarras = item.CodigoDeBarras,
                        DataVencimento = item.DataVencimento,
                        Quantidade = item.Quantidade
                    });
                }
                else
                {
                    alimento.Quantidade += item.Quantidade;
                }
            }

            distribuicao.Status = "Cancelada";
            distribuicao.Beneficiario.StatusFila = "Aguardando";
            distribuicao.Beneficiario.HorarioRetirada = null;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["MensagemSucesso"] = "Reserva cancelada e itens devolvidos ao estoque.";
            return RedirectToAction("Doacoes");
        }
    }
}
        