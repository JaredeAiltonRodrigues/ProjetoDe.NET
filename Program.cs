using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using OngDoacao.Data;
using OngDoacao.Servicos;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do MVC
builder.Services.AddControllersWithViews();

// Configura o Banco de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra os Serviços (Email e IA)
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddHttpClient<IaService>();

// --- SISTEMA DE LOGIN (CRACHÁ VIRTUAL) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AcessoNegado";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Aceita rodar no localhost sem HTTPS
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// A ORDEM AQUI É VITAL: Primeiro descobre quem é, depois autoriza!
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();