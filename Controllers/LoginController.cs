using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OngDoacao.Data;

namespace OngDoacao.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Entrar(string nome, string senha)
        {
            // Busca o usuário no banco de dados
            var usuario = _context.Equipe.FirstOrDefault(u => u.Nome == nome && u.Senha == senha && u.Ativo);

            if (usuario == null)
            {
                // Se for a primeira vez rodando e não tiver usuário, cria um Admin Master provisório para você não ficar trancado fora do sistema
                if (nome == "admin" && senha == "admin123") {
                    usuario = new Models.MembroEquipe { Nome = "Administrador", PerfilAcesso = "Admin" };
                }
                else {
                    TempData["Erro"] = "Nome ou senha incorretos!";
                    return RedirectToAction("Index");
                }
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

        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}