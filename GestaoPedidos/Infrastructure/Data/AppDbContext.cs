using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Entities.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
        public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
        public DbSet<MensagemWhatsApp> MensagensWhatsApp => Set<MensagemWhatsApp>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // CLIENTE
            // ==========================================

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(c => c.Nome)
                    .HasMaxLength(120);

                entity.Property(c => c.Email)
                    .HasMaxLength(254);

                entity.Property(c => c.Cpf)
                    .HasMaxLength(11)
                    .IsFixedLength();

                entity.HasIndex(c => c.Email)
                    .IsUnique();

                entity.HasIndex(c => c.Cpf)
                    .IsUnique();
            });

            // ==========================================
            // USUARIO
            // ==========================================

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.Nome)
                    .HasMaxLength(120);

                entity.Property(u => u.Email)
                    .HasMaxLength(254);

                entity.Property(u => u.Senha)
                    .HasMaxLength(100);

                entity.Property(u => u.VersaoToken)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // ==========================================
            // PRODUTO
            // ==========================================

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.Property(p => p.Nome)
                    .HasMaxLength(160);

                entity.Property(p => p.Marca)
                    .HasMaxLength(120);

                entity.Property(p => p.Preco)
                    .HasPrecision(18, 2);

                entity.Property(p => p.Versao)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .IsConcurrencyToken();

                entity.ToTable("Produtos", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Produtos_Estoque_NaoNegativo",
                        "\"Estoque\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Produtos_Reserva_NaoNegativa",
                        "\"QuantidadeReservada\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Produtos_CompraPendente_NaoNegativa",
                        "\"QuantidadeCompradaPendente\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Produtos_Preco_Positivo",
                        "\"Preco\" > 0");
                });
            });

            // ==========================================
            // PEDIDO
            // ==========================================

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Status)
                    .HasConversion<int>();

                entity.HasOne(p => p.Cliente)
                    .WithMany()
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // PEDIDO ITEM
            // ==========================================

            modelBuilder.Entity<PedidoItem>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Preco)
                    .HasPrecision(18, 2);

                entity.HasIndex(i => new
                {
                    i.PedidoId,
                    i.ProdutoId
                })
                .IsUnique();

                entity.HasOne<Produto>()
                    .WithMany()
                    .HasForeignKey(i => i.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("PedidoItens", table =>
                {
                    table.HasCheckConstraint(
                        "CK_PedidoItens_Quantidade_Positiva",
                        "\"Quantidade\" > 0");

                    table.HasCheckConstraint(
                        "CK_PedidoItens_Preco_Positivo",
                        "\"Preco\" > 0");
                });
            });

            // ==========================================
            // MOVIMENTAÇÃO DE ESTOQUE
            // ==========================================

            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Observacao)
                    .HasMaxLength(400);

                entity.Property(m => m.ProdutoNome)
                    .HasMaxLength(160);

                entity.Property(m => m.TipoMovimentacao)
                    .HasConversion<int>();

                entity.Property(m => m.OrigemMovimentacao)
                    .HasConversion<int>();

                entity.HasIndex(m => new
                {
                    m.ProdutoId,
                    m.DataMovimentacao
                })
                .IsDescending(false, true);

                entity.HasOne(m => m.Produto)
                    .WithMany()
                    .HasForeignKey(m => m.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("MovimentacoesEstoque", table =>
                {
                    table.HasCheckConstraint(
                        "CK_MovimentacoesEstoque_Quantidade_Positiva",
                        "\"Quantidade\" > 0");
                });
            });

            // ==========================================
            // WHATSAPP
            // ==========================================

            modelBuilder.Entity<MensagemWhatsApp>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.IdMensagemWhatsApp)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoIdMensagem);

                // A Meta fornece um ID único para cada mensagem.
                // Impede que a mesma mensagem seja salva duas vezes.
                entity.HasIndex(m => m.IdMensagemWhatsApp)
                    .IsUnique();

                entity.Property(m => m.NumeroRemetente)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoNumeroRemetente);

                entity.Property(m => m.NomeRemetente)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoNomeRemetente);

                entity.Property(m => m.TipoMensagem)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoTipoMensagem);

                entity.Property(m => m.Conteudo)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoConteudo);

                entity.Property(m => m.DataMensagem)
                    .IsRequired();

                entity.Property(m => m.PhoneNumberId)
                    .IsRequired()
                    .HasMaxLength(MensagemWhatsApp.TamanhoMaximoPhoneNumberId);

                entity.HasIndex(m => m.NumeroRemetente);

                entity.HasIndex(m => m.DataMensagem);
            });
        }
    }
}
