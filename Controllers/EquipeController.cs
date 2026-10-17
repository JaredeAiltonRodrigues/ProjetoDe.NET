using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    // Apenas o Administrador pode acessar o RH
    [Authorize(Roles = "Admin")]
    public class EquipeController : Controller
    {
        private readonly AppDbContext _context;

        public EquipeController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lista todos os membros cadastrados
        public IActionResult Index()
        {
            var equipe = _context.Equipe.ToList();
            return View(equipe);
        }

        // 2. Tela de Cadastro vazia
        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View();
        }

        // 3. Salva o novo voluntário/funcionário no banco
        [HttpPost]
        public IActionResult Cadastrar(MembroEquipe membro)
        {
            membro.DataEntrada = DateTime.Now;
            membro.Ativo = true;

            _context.Equipe.Add(membro);
            _context.SaveChanges();

            TempData["MensagemSucesso"] = "Novo membro cadastrado com sucesso! Ele já pode fazer login.";
            return RedirectToAction("Index");
        }
    }
}