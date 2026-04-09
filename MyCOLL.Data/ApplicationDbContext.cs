using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Shared; 
namespace MyCOLL.Data
{
    // Herda de IdentityDbContext para incluir as tabelas de Users (Login)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- AS TUAS TABELAS ---
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ModoDisponibilizacao> ModosDisponibilizacao { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<DetalheEncomenda> DetalhesEncomenda { get; set; }
        // Se já criaste a classe Favorito, descomenta a linha abaixo:
        // public DbSet<Favorito> Favoritos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração da relação Categoria Pai -> Filhas
            builder.Entity<Categoria>()
                .HasOne(c => c.CategoriaPai)
                .WithMany(c => c.SubCategorias)
                .HasForeignKey(c => c.CategoriaPaiId)
                .OnDelete(DeleteBehavior.Restrict);

                // 2. CONFIGURAÇÃO CRÍTICA: Ligar Encomenda ao Utilizador
            // Como no Shared removemos o objeto "ApplicationUser", temos de fazer a ligação manual aqui
            builder.Entity<Encomenda>()
                .HasOne<ApplicationUser>()    // A Encomenda tem um Utilizador real
                .WithMany()                   // O Utilizador tem muitas encomendas (sem navegação explícita)
                .HasForeignKey(e => e.ClienteId) // A ligação é feita através deste campo string
                .IsRequired()                 // É obrigatório ter cliente
                .OnDelete(DeleteBehavior.Restrict); // Se apagares o User, NÃO apaga o histórico (Segurança)

            // Configuração de Precisão para Preços (Obrigatório para evitar avisos)
            builder.Entity<Produto>().Property(p => p.PrecoBase).HasColumnType("decimal(18,2)");
            builder.Entity<Produto>().Property(p => p.MargemLucro).HasColumnType("decimal(18,2)");
            builder.Entity<Produto>().Property(p => p.PrecoVenda).HasColumnType("decimal(18,2)");
            // builder.Entity<Encomenda>().Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");
            builder.Entity<DetalheEncomenda>().Property(d => d.PrecoUnitario).HasColumnType("decimal(18,2)");
        }
    }
}