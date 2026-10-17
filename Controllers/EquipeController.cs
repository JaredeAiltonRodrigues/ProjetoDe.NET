using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher<MembroEquipe> _passwordHasher;

        public EquipeController(AppDbContext context, IPasswordHasher<MembroEquipe> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // 1. Lista todos os membros cadastrados
        public IActionResult Index()
        {
            var equipe = _context.Equipe.OrderBy(m => m.Nome).ToList();
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
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(MembroEquipe membro)
        {
            if (membro.Nome?.Length > 120)
            {
                ModelState.AddModelError(nameof(membro.Nome), "O nome de usuário deve ter até 120 caracteres.");
            }

            if (membro.CargoOuFuncao?.Length > 100)
            {
                ModelState.AddModelError(nameof(membro.CargoOuFuncao), "A função deve ter até 100 caracteres.");
            }

            if (membro.Telefone?.Length > 30)
            {
                ModelState.AddModelError(nameof(membro.Telefone), "O telefone deve ter até 30 caracteres.");
            }

            if (membro.Senha?.Length > 200)
            {
                ModelState.AddModelError(nameof(membro.Senha), "A senha deve ter no máximo 200 caracteres.");
            }

            if (membro.TipoVinculo is not ("Voluntário" or "Funcionário Remunerado"))
            {
                ModelState.AddModelError(nameof(membro.TipoVinculo), "Selecione um tipo de vínculo válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(membro);
            }

            if (_context.Equipe.Any(m => m.Nome == membro.Nome))
            {
                ModelState.AddModelError(nameof(membro.Nome), "Já existe um membro com esse nome de usuário.");
                return View(membro);
            }

            membro.DataEntrada = DateTime.Now;
            membro.Ativo = true;
            membro.Senha = _passwordHasher.HashPassword(membro, membro.Senha!);

            _context.Equipe.Add(membro);
            _context.SaveChanges();

            TempData["MensagemSucesso"] = "Novo membro cadastrado com sucesso! Ele já pode fazer login.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlterarStatus(int id)
        {
            var membro = _context.Equipe.Find(id);
            if (membro == null)
            {
                return NotFound();
            }

            if (membro.Ativo && membro.PerfilAcesso == "Admin" &&
                _context.Equipe.Count(m => m.Ativo && m.PerfilAcesso == "Admin") == 1)
            {
                TempData["Erro"] = "O último administrador ativo não pode ser desativado.";
                return RedirectToAction("Index");
            }

            membro.Ativo = !membro.Ativo;
            _context.SaveChanges();
            TempData["MensagemSucesso"] = membro.Ativo
                ? $"Acesso de {membro.Nome} ativado."
                : $"Acesso de {membro.Nome} desativado.";
            return RedirectToAction("Index");
        }
    }
}