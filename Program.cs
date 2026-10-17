using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OngDoacao.Data;
using OngDoacao.Models;
using OngDoacao.Servicos;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do MVC
builder.Services.AddControllersWithViews();

// Configura o Banco de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPasswordHasher<MembroEquipe>, PasswordHasher<MembroEquipe>>();

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
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

var bootstrapAdminName = app.Configuration["BootstrapAdmin:Name"];
var bootstrapAdminPassword = app.Configuration["BootstrapAdmin:Password"];
var hasBootstrapAdminName = !string.IsNullOrWhiteSpace(bootstrapAdminName);
var hasBootstrapAdminPassword = !string.IsNullOrWhiteSpace(bootstrapAdminPassword);

if (hasBootstrapAdminName != hasBootstrapAdminPassword)
{
    throw new InvalidOperationException("Configure BootstrapAdmin:Name e BootstrapAdmin:Password juntos.");
}

var applyMigrations = app.Environment.IsDevelopment() ||
                      app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup") ||
                      hasBootstrapAdminName;

if (applyMigrations)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (hasBootstrapAdminName && hasBootstrapAdminPassword)
    {
        var password = bootstrapAdminPassword!;
        if (password.Length < 12)
        {
            throw new InvalidOperationException("BootstrapAdmin:Password deve ter pelo menos 12 caracteres.");
        }

        if (!await db.Equipe.AnyAsync(m => m.Ativo && m.PerfilAcesso == "Admin"))
        {
            var admin = new MembroEquipe
            {
                Nome = bootstrapAdminName,
                CargoOuFuncao = "Administrador",
                TipoVinculo = "Equipe",
                PerfilAcesso = "Admin",
                Ativo = true
            };
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<MembroEquipe>>();
            admin.Senha = passwordHasher.HashPassword(admin, password);
            db.Equipe.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}

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