using InfoSistemas.LicencaAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Data;

public class LicencaDbContext(DbContextOptions<LicencaDbContext> options) : DbContext(options)
{
    public DbSet<Cliente>     Clientes     => Set<Cliente>();
    public DbSet<Licenca>     Licencas     => Set<Licenca>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Renovacao>   Renovacoes   => Set<Renovacao>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        // MySQL: charset padrao utf8mb4 para suporte completo a Unicode
        m.HasCharSet("utf8mb4");

        // Cliente
        m.Entity<Cliente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RazaoSocial).HasMaxLength(100).IsRequired();
            e.Property(x => x.Fantasia).HasMaxLength(100);
            e.Property(x => x.Cnpj).HasMaxLength(18);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.Fone).HasMaxLength(20);
            e.Property(x => x.Endereco).HasMaxLength(150);
            e.Property(x => x.Cidade).HasMaxLength(80);
            e.Property(x => x.Uf).HasMaxLength(2);
            e.Property(x => x.Cep).HasMaxLength(10);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("ATIVO");
            e.Property(x => x.Plano).HasMaxLength(20).HasDefaultValue("BASICO");
            e.Property(x => x.MotivoBloqueio).HasMaxLength(255);

            e.HasOne(x => x.Licenca)
             .WithOne(x => x.Cliente)
             .HasForeignKey<Licenca>(x => x.ClienteId);

            e.HasMany(x => x.Dispositivos)
             .WithOne(x => x.Cliente)
             .HasForeignKey(x => x.ClienteId);

            e.HasMany(x => x.Renovacoes)
             .WithOne(x => x.Cliente)
             .HasForeignKey(x => x.ClienteId);
        });

        // Licenca
        m.Entity<Licenca>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Chave).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Chave).IsUnique();
        });

        // Dispositivo
        m.Entity<Dispositivo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MachineId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(10).HasDefaultValue("DESKTOP");
            e.Property(x => x.Nome).HasMaxLength(80);
            e.Property(x => x.AppVersion).HasMaxLength(20);
            // Indice unico: um mesmo MachineId nao pode ter dois registros ativos pro mesmo cliente
            e.HasIndex(x => new { x.ClienteId, x.MachineId }).IsUnique();
        });

        // Renovacao
        m.Entity<Renovacao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Responsavel).HasMaxLength(80);
            e.Property(x => x.Observacao).HasMaxLength(255);
        });
    }
}
