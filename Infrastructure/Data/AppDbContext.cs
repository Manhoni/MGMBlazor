using Microsoft.EntityFrameworkCore;
using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Estas classes se tornarão tabelas no Postgres
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<NotaFiscalEmitida> NotasFiscaisEmitidas { get; set; }
    // Se você quiser salvar os detalhes dos serviços e da nota bruta, adicione-os aqui:
    public DbSet<Servico> Servicos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Exemplo: Garante que não existam duas notas com o mesmo número no banco
        modelBuilder.Entity<NotaFiscalEmitida>()
            .HasIndex(n => n.NumeroNota)
            .IsUnique();
    }
}