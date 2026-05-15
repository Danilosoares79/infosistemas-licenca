using InfoSistemas.LicencaAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Services;

public class AlertaService(LicencaDbContext db, ILogger<AlertaService> logger)
{
    // Verifica clientes expirados e atualiza status
    public async Task VerificarExpiradosAsync()
    {
        var expirados = await db.Clientes
            .Where(c => c.Status == "ATIVO" && c.DataVencimento.Date < DateTime.UtcNow.Date)
            .ToListAsync();

        foreach (var c in expirados)
        {
            c.Status = "EXPIRADO";
            logger.LogWarning("Cliente {Id} - {Razao} expirado em {Data}",
                c.Id, c.RazaoSocial, c.DataVencimento);
        }

        if (expirados.Count > 0)
            await db.SaveChangesAsync();

        logger.LogInformation("{Count} clientes marcados como EXPIRADO.", expirados.Count);
    }
}

// Servico background — roda a cada hora para checar expirados
public class AlertaBackgroundService(IServiceProvider services, ILogger<AlertaBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("AlertaBackgroundService iniciado.");
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<AlertaService>();
                await svc.VerificarExpiradosAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no AlertaBackgroundService.");
            }
            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }
}
