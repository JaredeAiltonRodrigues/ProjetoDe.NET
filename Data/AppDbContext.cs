using Microsoft.EntityFrameworkCore;
using OngDoacao.Models;

namespace OngDoacao.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Alimento> Alimentos { get; set; }
        public DbSet<Beneficiario> Beneficiarios { get; set; }

        // Novas tabelas adicionadas para a Gestão Completa da ONG
        public DbSet<MembroEquipe> Equipe { get; set; }
        public DbSet<TransacaoFinanceira> Financeiro { get; set; }
        public DbSet<EscalaVoluntario> Escalas { get; set; }
    }
}   