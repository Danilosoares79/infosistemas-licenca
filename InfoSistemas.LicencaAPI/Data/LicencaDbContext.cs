using InfoSistemas.LicencaAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Data;

public class LicencaDbContext(DbContextOptions<LicencaDbContext> options) : DbContext(options)
{
    public DbSet<Cliente>     Clientes     => Set<Cliente>();
    public DbSet<Licenca>     Licencas     => Set<Licenca>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Renovacao>   Renovacoes   => Set<Renovacao>();
    public DbSet<Pagamento>   Pagamentos   => Set<Pagamento>();
    public DbSet<Usuario>     Usuarios     => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.HasCharSet("utf8mb4");

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
            e.Property(x => x.RenovacaoAutomatica).HasDefaultValue(false);

            e.HasOne(x => x.Licenca).WithOne(x => x.Cliente).HasForeignKey<Licenca>(x => x.ClienteId);
            e.HasMany(x => x.Dispositivos).WithOne(x => x.Cliente).HasForeignKey(x => x.ClienteId);
            e.HasMany(x => x.Renovacoes).WithOne(x => x.Cliente).HasForeignKey(x => x.ClienteId);
            e.HasMany(x => x.Pagamentos).WithOne(x => x.Cliente).HasForeignKey(x => x.ClienteId);
        });

        m.Entity<Licenca>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Chave).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Chave).IsUnique();
        });

        m.Entity<Dispositivo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MachineId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(10).HasDefaultValue("DESKTOP");
            e.Property(x => x.Nome).HasMaxLength(80);
            e.Property(x => x.AppVersion).HasMaxLength(20);
            e.HasIndex(x => new { x.ClienteId, x.MachineId }).IsUnique();
        });

        m.Entity<Renovacao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Responsavel).HasMaxLength(80);
            e.Property(x => x.Observacao).HasMaxLength(255);
        });

        m.Entity<Pagamento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Valor).HasPrecision(10, 2);
            e.Property(x => x.FormaPagamento).HasMaxLength(20).HasDefaultValue("PIX");
            e.Property(x => x.Observacao).HasMaxLength(255);
            e.Property(x => x.Responsavel).HasMaxLength(80);
        });

        m.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Login).HasMaxLength(50).IsRequired();
            e.Property(x => x.SenhaHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Perfil).HasMaxLength(20).HasDefaultValue("Admin");
            e.HasIndex(x => x.Login).IsUnique();
        });
    }
}
