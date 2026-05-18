using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/admin/clientes")]
public class ClienteController(LicencaService svc, IConfiguration cfg) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (!Ok()) return Unauthorized();
        return Ok(await svc.ListarClientesAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        if (!Ok()) return Unauthorized();
        var c = await svc.ObterClienteAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest req)
    {
        if (!Ok()) return Unauthorized();
        var c = await svc.CriarClienteAsync(req);
        return CreatedAtAction(nameof(Obter), new { id = c.Id }, c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarClienteRequest req)
    {
        if (!Ok()) return Unauthorized();
        await svc.AtualizarClienteAsync(id, req);
        return NoContent();
    }

    [HttpPost("{id:int}/bloquear")]
    public async Task<IActionResult> Bloquear(int id, [FromBody] BloquearRequest req)
    {
        if (!Ok()) return Unauthorized();
        await svc.BloquearAsync(id, req);
        return Ok(new { mensagem = "Cliente bloqueado." });
    }

    [HttpPost("{id:int}/desbloquear")]
    public async Task<IActionResult> Desbloquear(int id)
    {
        if (!Ok()) return Unauthorized();
        await svc.DesbloquearAsync(id);
        return Ok(new { mensagem = "Cliente desbloqueado." });
    }

    [HttpPost("{id:int}/renovar")]
    public async Task<IActionResult> Renovar(int id, [FromBody] RenovarRequest req)
    {
        if (!Ok()) return Unauthorized();
        return Ok(await svc.RenovarAsync(id, req));
    }

    // ← ENDPOINT QUE ESTAVA FALTANDO E CAUSAVA O TRAVAMENTO
    [HttpGet("{id:int}/dispositivos")]
    public async Task<IActionResult> Dispositivos(int id)
    {
        if (!Ok()) return Unauthorized();
        return Ok(await svc.ListarDispositivosAsync(id));
    }

    [HttpDelete("{id:int}/dispositivos/{machineId}")]
    public async Task<IActionResult> RemoverDispositivo(int id, string machineId)
    {
        if (!Ok()) return Unauthorized();
        await svc.RemoverDispositivoAsync(id, machineId);
        return NoContent();
    }

    [HttpPost("{id:int}/remover-dispositivos-excedentes")]
    public async Task<IActionResult> RemoverExcedentes(int id, [FromBody] RemoverExcedentesRequest req)
    {
        if (!Ok()) return Unauthorized();
        var removidos = await svc.RemoverDispositivosExcedentesAsync(id, req.Tipo, req.NovoLimite);
        return Ok(new { removidos, mensagem = $"{removidos} dispositivo(s) removido(s)." });
    }

    [HttpGet("/api/admin/vencimentos")]
    public async Task<IActionResult> Vencimentos([FromQuery] int dias = 7)
    {
        if (!Ok()) return Unauthorized();
        return Ok(await svc.VencimentosProximosAsync(dias));
    }

    [HttpPost("{id:int}/renovacao-automatica")]
    public async Task<IActionResult> RenovacaoAutomatica(int id, [FromBody] bool ativo)
    {
        if (!Ok()) return Unauthorized();
        await svc.AtualizarRenovacaoAutomaticaAsync(id, ativo);
        return Ok(new { mensagem = ativo ? "Renovacao automatica ativada." : "Renovacao automatica desativada." });
    }

    [HttpGet("{id:int}/licenca/download")]
    public async Task<IActionResult> DownloadLic(int id)
    {
        if (!Ok()) return Unauthorized();
        var lic = await svc.GerarArquivoLicAsync(id);
        var bytes = System.Text.Encoding.UTF8.GetBytes(lic.Conteudo);
        return File(bytes, "application/octet-stream", lic.NomeArquivo);
    }

    private bool Ok()
    {
        Request.Headers.TryGetValue("X-Admin-Key", out var key);
        return key == cfg["AdminKey"];
    }
}
