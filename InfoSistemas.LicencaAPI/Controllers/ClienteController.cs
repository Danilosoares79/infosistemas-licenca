using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/admin/clientes")]
public class ClienteController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    // GET /api/admin/clientes
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (!AdminKeyValida()) return Unauthorized();
        return Ok(await svc.ListarClientesAsync());
    }

    // GET /api/admin/clientes/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var c = await svc.ObterClienteAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    // POST /api/admin/clientes
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var c = await svc.CriarClienteAsync(req);
        return CreatedAtAction(nameof(Obter), new { id = c.Id }, c);
    }

    // PUT /api/admin/clientes/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarClienteRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        await svc.AtualizarClienteAsync(id, req);
        return NoContent();
    }

    // POST /api/admin/clientes/{id}/bloquear
    [HttpPost("{id:int}/bloquear")]
    public async Task<IActionResult> Bloquear(int id, [FromBody] BloquearRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        await svc.BloquearAsync(id, req);
        return Ok(new { mensagem = "Cliente bloqueado com sucesso." });
    }

    // POST /api/admin/clientes/{id}/desbloquear
    [HttpPost("{id:int}/desbloquear")]
    public async Task<IActionResult> Desbloquear(int id)
    {
        if (!AdminKeyValida()) return Unauthorized();
        await svc.DesbloquearAsync(id);
        return Ok(new { mensagem = "Cliente desbloqueado com sucesso." });
    }

    // POST /api/admin/clientes/{id}/renovar
    [HttpPost("{id:int}/renovar")]
    public async Task<IActionResult> Renovar(int id, [FromBody] RenovarRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var r = await svc.RenovarAsync(id, req);
        return Ok(r);
    }

    // GET /api/admin/clientes/{id}/dispositivos
    [HttpGet("{id:int}/dispositivos")]
    public async Task<IActionResult> Dispositivos(int id)
    {
        if (!AdminKeyValida()) return Unauthorized();
        return Ok(await svc.ListarDispositivosAsync(id));
    }

    // GET /api/admin/vencimentos
    [HttpGet("/api/admin/vencimentos")]
    public async Task<IActionResult> Vencimentos([FromQuery] int dias = 7)
    {
        if (!AdminKeyValida()) return Unauthorized();
        return Ok(await svc.VencimentosProximosAsync(dias));
    }

    private bool AdminKeyValida()
    {
        Request.Headers.TryGetValue("X-Admin-Key", out var key);
        return key == cfg["AdminKey"];
    }
}
