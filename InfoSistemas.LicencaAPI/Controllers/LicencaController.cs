using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/licenca")]
public class LicencaController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    // POST /api/licenca/validar
    // Chamado pelo PDV ao abrir o sistema (ja instalado e ativado)
    [HttpPost("validar")]
    public async Task<IActionResult> Validar([FromBody] ValidarLicencaRequest req)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.ValidarAsync(req);
        return Ok(result);
    }

    // POST /api/licenca/ativar
    // Chamado pelo PDV na PRIMEIRA instalacao ao importar o arquivo .lic
    // Vincula o ActivationToken ao MachineId — arquivo so pode ativar 1 PC!
    [HttpPost("ativar")]
    public async Task<IActionResult> Ativar([FromBody] AtivarLicencaRequest req)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.AtivarLicencaAsync(req);
        if (!result.Sucesso) return BadRequest(result);
        return Ok(result);
    }

    // POST /api/licenca/heartbeat
    // Chamado a cada 15 minutos pelo PDV (ja instalado)
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest req)
    {
        if (!ApiKeyValida()) return Unauthorized("Chave de API invalida.");
        var result = await svc.HeartbeatAsync(req);
        return Ok(result);
    }

    // GET /api/licenca/status/{chave}
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
