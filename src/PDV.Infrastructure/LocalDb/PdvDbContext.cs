using Microsoft.EntityFrameworkCore;
using PDV.Core.Enums;
using PDV.Core.Models;

namespace PDV.Infrastructure.LocalDb;

public class PdvDbContext : DbContext
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<MovimentoCaixa> MovimentosCaixa => Set<MovimentoCaixa>();
    public DbSet<Operador> Operadores => Set<Operador>();

    public PdvDbContext(DbContextOptions<PdvDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================
        // PRODUTO
        // ============================
        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("produtos");
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.CodigoBarras);
            e.HasIndex(p => p.CodigoInterno);
            e.Property(p => p.PrecoVenda).HasColumnType("decimal(18,2)");
            e.Property(p => p.EstoqueAtual).HasColumnType("decimal(18,3)");
            e.Property(p => p.AliquotaICMS).HasColumnType("decimal(18,2)");
        });

        // ============================
        // VENDA
        // ============================
        modelBuilder.Entity<Venda>(e =>
        {
            e.ToTable("vendas");
            e.HasKey(v => v.Id);
            e.HasIndex(v => v.NumeroVenda).IsUnique();
            e.HasIndex(v => v.SincronizadoERP);

            e.Property(v => v.DescontoTotal).HasColumnType("decimal(18,2)");
            e.Property(v => v.AcrescimoTotal).HasColumnType("decimal(18,2)");
            e.Property(v => v.Status).HasConversion<int>();

            // Propriedades computadas - ignorar no banco
            e.Ignore(v => v.SubTotal);
            e.Ignore(v => v.ValorTotal);

            e.HasMany(v => v.Itens)
                .WithOne()
                .HasForeignKey(i => i.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(v => v.Pagamentos)
                .WithOne()
                .HasForeignKey(p => p.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================
        // ITEM VENDA
        // ============================
        modelBuilder.Entity<ItemVenda>(e =>
        {
            e.ToTable("itens_venda");
            e.HasKey(i => i.Id);

            e.Property(i => i.Quantidade).HasColumnType("decimal(18,3)");
            e.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
            e.Property(i => i.DescontoPercentual).HasColumnType("decimal(18,2)");
            e.Property(i => i.DescontoValor).HasColumnType("decimal(18,2)");
            e.Property(i => i.AliquotaICMS).HasColumnType("decimal(18,2)");

            e.Ignore(i => i.ValorTotal);
            e.Ignore(i => i.ValorICMS);
            e.Ignore(i => i.Produto);
        });

        // ============================
        // PAGAMENTO
        // ============================
        modelBuilder.Entity<Pagamento>(e =>
        {
            e.ToTable("pagamentos");
            e.HasKey(p => p.Id);

            e.Property(p => p.Valor).HasColumnType("decimal(18,2)");
            e.Property(p => p.ValorRecebido).HasColumnType("decimal(18,2)");
            e.Property(p => p.FormaPagamento).HasConversion<int>();

            e.Ignore(p => p.Troco);
        });

        // ============================
        // CAIXA
        // ============================
        modelBuilder.Entity<Caixa>(e =>
        {
            e.ToTable("caixas");
            e.HasKey(c => c.Id);

            e.Property(c => c.ValorAbertura).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalVendas).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalDinheiro).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalCartaoCredito).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalCartaoDebito).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalPix).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalSangria).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalSuprimento).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalCancelamentos).HasColumnType("decimal(18,2)");
            e.Property(c => c.ValorFechamento).HasColumnType("decimal(18,2)");

            e.Ignore(c => c.SaldoEsperado);
            e.Ignore(c => c.Diferenca);
            e.Ignore(c => c.Aberto);

            e.HasMany(c => c.Movimentos)
                .WithOne()
                .HasForeignKey(m => m.CaixaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================
        // MOVIMENTO CAIXA
        // ============================
        modelBuilder.Entity<MovimentoCaixa>(e =>
        {
            e.ToTable("movimentos_caixa");
            e.HasKey(m => m.Id);
            e.Property(m => m.Valor).HasColumnType("decimal(18,2)");
            e.Property(m => m.Tipo).HasConversion<int>();
        });

        // ============================
        // OPERADOR
        // ============================
        modelBuilder.Entity<Operador>(e =>
        {
            e.ToTable("operadores");
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.Login).IsUnique();
        });

        // Seed de dados iniciais
        modelBuilder.Entity<Operador>().HasData(
            new Operador { Id = 1, Nome = "Administrador", Login = "admin", Perfil = "admin", Ativo = true },
            new Operador { Id = 2, Nome = "Caixa 01", Login = "caixa1", Perfil = "caixa", Ativo = true },
            new Operador { Id = 3, Nome = "Caixa 02", Login = "caixa2", Perfil = "caixa", Ativo = true }
        );
    }
}
