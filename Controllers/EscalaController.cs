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
        public IActionResult Agendar(EscalaVoluntario escala)
        {
            _context.Escalas.Add(escala);
            _context.SaveChanges();
            TempData["MensagemSucesso"] = "Turno agendado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
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