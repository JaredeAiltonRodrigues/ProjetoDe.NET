using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EscalaController : Controller
    {
        private readonly AppDbContext _context;

        public EscalaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Traz a escala dos próximos 15 dias, incluindo os dados do voluntário
            var escalas = _context.Escalas
                .Include(e => e.Membro)
                .Where(e => e.DataTurno >= DateTime.Today)
                .OrderBy(e => e.DataTurno)
                .ToList();

            ViewBag.Equipe = _context.Equipe.Where(e => e.Ativo).ToList();
            return View(escalas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agendar(EscalaVoluntario escala)
        {
            if (!ModelState.IsValid || escala.DataTurno <= DateTime.Now)
            {
                TempData["Erro"] = "Informe um turno válido para uma data futura.";
                return RedirectToAction("Index");
            }

            var membroAtivo = _context.Equipe.Any(m => m.Id == escala.MembroEquipeId && m.Ativo);
            var setorPermitido = new[] { "Triagem de Alimentos", "Entrega de Cestas", "Administrativo" }
                .Contains(escala.Setor);
            var conflito = _context.Escalas.Any(e => e.MembroEquipeId == escala.MembroEquipeId && e.DataTurno == escala.DataTurno);

            if (!membroAtivo || !setorPermitido || conflito)
            {
                TempData["Erro"] = conflito
                    ? "Este membro já possui um turno nesse horário."
                    : "Selecione um membro ativo e um setor válido.";
                return RedirectToAction("Index");
            }

            _context.Escalas.Add(escala);
            _context.SaveChanges();
            TempData["MensagemSucesso"] = "Turno agendado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remover(int id)
        {
            var escala = _context.Escalas.Find(id);
            if (escala != null)
            {
                _context.Escalas.Remove(escala);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}