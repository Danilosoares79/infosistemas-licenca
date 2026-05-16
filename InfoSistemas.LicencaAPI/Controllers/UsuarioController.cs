using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
public class UsuarioController(AuthService svc, IConfiguration cfg) : ControllerBase
{
    // GET /api/admin/usuarios
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (!AdminKeyValida()) return Unauthorized();
        return Ok(await svc.ListarAsync());
    }

    // GET /api/admin/usuarios/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var u = await svc.ObterAsync(id);
        return u == null ? NotFound() : Ok(u);
    }

    // POST /api/admin/usuarios
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var (sucesso, erro, usuario) = await svc.CriarAsync(req);
        if (!sucesso) return BadRequest(new { erro });
        return CreatedAtAction(nameof(Obter), new { id = usuario!.Id }, usuario);
    }

    // PUT /api/admin/usuarios/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var (sucesso, erro) = await svc.AtualizarAsync(id, req);
        if (!sucesso) return BadRequest(new { erro });
        return NoContent();
    }

    // POST /api/admin/usuarios/{id}/alterar-senha
    [HttpPost("{id:int}/alterar-senha")]
    public async Task<IActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var (sucesso, erro) = await svc.AlterarSenhaAsync(id, req);
        if (!sucesso) return BadRequest(new { erro });
        return NoContent();
    }

    // POST /api/admin/usuarios/{id}/resetar-senha
    [HttpPost("{id:int}/resetar-senha")]
    public async Task<IActionResult> ResetarSenha(int id, [FromBody] ResetarSenhaRequest req)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var (sucesso, erro) = await svc.ResetarSenhaAsync(id, req);
        if (!sucesso) return BadRequest(new { erro });
        return NoContent();
    }

    // DELETE /api/admin/usuarios/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        if (!AdminKeyValida()) return Unauthorized();
        var (sucesso, erro) = await svc.RemoverAsync(id);
        if (!sucesso) return BadRequest(new { erro });
        return NoContent();
    }

    private bool AdminKeyValida()
    {
        Request.Headers.TryGetValue("X-Admin-Key", out var key);
        return key == cfg["AdminKey"];
    }
}
