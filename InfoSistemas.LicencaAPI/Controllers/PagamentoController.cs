using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/admin/pagamentos")]
public class PagamentoController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    // GET /api/admin/pagamentos?clienteId=1
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? clienteId = null)
    {
        if (!Ok()) return Unauthorized();
        return Ok(await svc.ListarPagamentosAsync(clienteId));
    }

    // POST /api/admin/pagamentos/{clienteId}
    [HttpPost("{clienteId:int}")]
    public async Task<IActionResult> Registrar(int clienteId, [FromBody] RegistrarPagamentoRequest req)
    {
        if (!Ok()) return Unauthorized();
        var p = await svc.RegistrarPagamentoAsync(clienteId, req);
        return Ok(p);
    }

    private bool Ok()
    {
        Request.Headers.TryGetValue("X-Admin-Key", out var key);
        return key == cfg["AdminKey"];
    }
}
