using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;
using OngDoacao.Models;

namespace OngDoacao.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<MembroEquipe> _passwordHasher;

        public LoginController(AppDbContext context, IPasswordHasher<MembroEquipe> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrar(string? nome, string? senha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(senha))
            {
                TempData["Erro"] = "Informe o usuário e a senha.";
                return RedirectToAction("Index");
            }

            var usuario = _context.Equipe.FirstOrDefault(u => u.Nome == nome && u.Ativo);
            if (usuario == null || !VerificarSenha(usuario, senha))
            {
                TempData["Erro"] = "Nome ou senha incorretos.";
                return RedirectToAction("Index");
            }

            // Cria a "Identidade" da pessoa logada (Crachá virtual)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome!),
                new Claim(ClaimTypes.Role, usuario.PerfilAcesso!) // Aqui define se é Admin, Estoque, etc.
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Faz o login de fato e salva o cookie no navegador
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }

        private bool VerificarSenha(MembroEquipe usuario, string senha)
        {
            var senhaArmazenada = usuario.Senha;
            if (string.IsNullOrEmpty(senhaArmazenada))
            {
                return false;
            }

            if (!senhaArmazenada.StartsWith("AQAAAA", StringComparison.Ordinal))
            {
                if (!string.Equals(senhaArmazenada, senha, StringComparison.Ordinal))
                {
                    return false;
                }

                usuario.Senha = _passwordHasher.HashPassword(usuario, senha);
                _context.SaveChanges();
                return true;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, senhaArmazenada, senha);
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Senha = _passwordHasher.HashPassword(usuario, senha);
                _context.SaveChanges();
            }

            return resultado != PasswordVerificationResult.Failed;
        }
    }
}