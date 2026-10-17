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
        public DbSet<DistribuicaoDoacao> Distribuicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DistribuicaoDoacao>()
                .HasOne(distribuicao => distribuicao.Beneficiario)
                .WithMany()
                .HasForeignKey(distribuicao => distribuicao.BeneficiarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DistribuicaoDoacao>()
                .HasMany(distribuicao => distribuicao.Itens)
                .WithOne(item => item.DistribuicaoDoacao)
                .HasForeignKey(item => item.DistribuicaoDoacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DistribuicaoDoacao>()
                .HasIndex(distribuicao => new { distribuicao.Status, distribuicao.HorarioRetirada });

            base.OnModelCreating(modelBuilder);
        }
    }
}   