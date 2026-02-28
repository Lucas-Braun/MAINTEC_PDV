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

        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, CodigoBarras = "7891000100101", CodigoInterno = "001", Descricao = "COCA-COLA LATA 350ML", UnidadeMedida = "UN", PrecoVenda = 5.49m, EstoqueAtual = 120, NCM = "22021000", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 2, CodigoBarras = "7891000100202", CodigoInterno = "002", Descricao = "GUARANA ANTARCTICA 2L", UnidadeMedida = "UN", PrecoVenda = 8.99m, EstoqueAtual = 45, NCM = "22021000", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 3, CodigoBarras = "7891000100303", CodigoInterno = "003", Descricao = "PAO DE FORMA INTEGRAL 500G", UnidadeMedida = "UN", PrecoVenda = 9.90m, EstoqueAtual = 30, NCM = "19052090", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 7, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 4, CodigoBarras = "7891000100404", CodigoInterno = "004", Descricao = "LEITE INTEGRAL 1L", UnidadeMedida = "UN", PrecoVenda = 6.79m, EstoqueAtual = 80, NCM = "04011010", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 0, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 5, CodigoBarras = "7891000100505", CodigoInterno = "005", Descricao = "ARROZ BRANCO TIPO 1 5KG", UnidadeMedida = "UN", PrecoVenda = 27.90m, EstoqueAtual = 3, NCM = "10063021", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 7, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 6, CodigoBarras = "7891000100606", CodigoInterno = "006", Descricao = "FEIJAO CARIOCA 1KG", UnidadeMedida = "UN", PrecoVenda = 8.49m, EstoqueAtual = 2, NCM = "07133319", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 7, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 7, CodigoBarras = "7891000100707", CodigoInterno = "007", Descricao = "CAFE TORRADO MOIDO 500G", UnidadeMedida = "UN", PrecoVenda = 18.90m, EstoqueAtual = 25, NCM = "09012100", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 8, CodigoBarras = "7891000100808", CodigoInterno = "008", Descricao = "ACUCAR CRISTAL 1KG", UnidadeMedida = "UN", PrecoVenda = 5.29m, EstoqueAtual = 50, NCM = "17019900", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 9, CodigoBarras = "7891000100909", CodigoInterno = "009", Descricao = "OLEO DE SOJA 900ML", UnidadeMedida = "UN", PrecoVenda = 7.99m, EstoqueAtual = 40, NCM = "15079011", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 10, CodigoBarras = "7891000101010", CodigoInterno = "010", Descricao = "MACARRAO ESPAGUETE 500G", UnidadeMedida = "UN", PrecoVenda = 4.59m, EstoqueAtual = 60, NCM = "19021100", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 11, CodigoBarras = "7891000101111", CodigoInterno = "011", Descricao = "SABONETE DOVE 90G", UnidadeMedida = "UN", PrecoVenda = 3.99m, EstoqueAtual = 1, NCM = "34011190", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 12, CodigoBarras = "7891000101212", CodigoInterno = "012", Descricao = "DETERGENTE LIMPOL 500ML", UnidadeMedida = "UN", PrecoVenda = 2.99m, EstoqueAtual = 70, NCM = "34022000", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 13, CodigoBarras = "7891000101313", CodigoInterno = "013", Descricao = "BISCOITO CREAM CRACKER 400G", UnidadeMedida = "UN", PrecoVenda = 6.49m, EstoqueAtual = 35, NCM = "19053100", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 14, CodigoBarras = "7891000101414", CodigoInterno = "014", Descricao = "MANTEIGA COM SAL 200G", UnidadeMedida = "UN", PrecoVenda = 12.90m, EstoqueAtual = 15, NCM = "04051010", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 7, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) },
            new Produto { Id = 15, CodigoBarras = "7891000101515", CodigoInterno = "015", Descricao = "PAPEL HIGIENICO 12 ROLOS", UnidadeMedida = "PCT", PrecoVenda = 19.90m, EstoqueAtual = 4, NCM = "48181000", CFOP = "5102", CST_ICMS = "00", AliquotaICMS = 18, Ativo = true, UltimaAtualizacao = new DateTime(2026, 1, 1) }
        );
    }
}
