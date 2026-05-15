using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/dispositivo")]
public class DispositivoController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    // DELETE /api/dispositivo/{clienteId}/{machineId}
    // Remove/desativa um dispositivo (chamado ao desinstalar o sistema)
    [HttpDelete("{clienteId:int}/{machineId}")]
    public async Task<IActionResult> Remover(int clienteId, string machineId)
    {
        if (!ApiKeyValida()) return Unauthorized();
        await svc.RemoverDispositivoAsync(clienteId, machineId);
        return Ok(new { mensagem = "Dispositivo removido." });
    }

    private bool ApiKeyValida()
    {
        Request.Headers.TryGetValue("X-Api-Key", out var key);
        return key == cfg["ApiKey"];
    }
}
