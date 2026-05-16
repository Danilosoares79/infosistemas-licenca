using InfoSistemas.LicencaAPI.Data;
using InfoSistemas.LicencaAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Services;

public class AlertaService(LicencaDbContext db, ILogger<AlertaService> logger)
{
    // Verifica expirados e executa renovacao automatica se configurada
    public async Task VerificarExpiradosAsync()
    {
        var hoje = DateTime.UtcNow.Date;

        var clientes = await db.Clientes
            .Where(c => c.Status == "ATIVO" && c.DataVencimento.Date < hoje)
            .ToListAsync();

        int expirados = 0, renovados = 0;

        foreach (var c in clientes)
        {
            if (c.RenovacaoAutomatica)
            {
                // Renova +30 dias automaticamente
                var dataAnterior = c.DataVencimento;
                c.DataVencimento = DateTime.UtcNow.AddDays(30);
                c.AtualizadoEm   = DateTime.UtcNow;

                db.Renovacoes.Add(new Renovacao
                {
                    ClienteId       = c.Id,
                    DataAnterior    = dataAnterior,
                    DataNova        = c.DataVencimento,
                    DiasAdicionados = 30,
                    Responsavel     = "Sistema",
                    Observacao      = "Renovacao automatica (+30 dias)"
                });

                logger.LogInformation("Cliente {Id} - {Razao}: renovado automaticamente por 30 dias.", c.Id, c.RazaoSocial);
                renovados++;
            }
            else
            {
                c.Status = "EXPIRADO";
                logger.LogWarning("Cliente {Id} - {Razao}: expirado em {Data}.", c.Id, c.RazaoSocial, c.DataVencimento);
                expirados++;
            }
        }

        if (expirados > 0 || renovados > 0)
            await db.SaveChangesAsync();

        logger.LogInformation("Verificacao concluida: {E} expirados, {R} renovados automaticamente.", expirados, renovados);
    }
}

// Servico background — roda a cada hora
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
