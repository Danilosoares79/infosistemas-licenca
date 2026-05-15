using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/licenca")]
public class LicencaController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    // POST /api/licenca/validar
    // Chamado pelo Desktop/Mobile ao abrir o sistema
    [HttpPost("validar")]
    public async Task<IActionResult> Validar([FromBody] ValidarLicencaRequest req)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.ValidarAsync(req);
        return Ok(result);
    }

    // POST /api/licenca/heartbeat
    // Chamado a cada 15 minutos pelo Desktop/Mobile
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest req)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.HeartbeatAsync(req);
        return Ok(result);
    }

    // GET /api/licenca/status/{chave}
    // Consulta rapida de status (uso interno)
    [HttpGet("status/{chave}")]
    public async Task<IActionResult> Status(string chave)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.HeartbeatAsync(new HeartbeatRequest(chave, "STATUS-CHECK"));
        return Ok(result);
    }

    private bool ApiKeyValida()
    {
        Request.Headers.TryGetValue("X-Api-Key", out var key);
        return key == cfg["ApiKey"];
    }
}
