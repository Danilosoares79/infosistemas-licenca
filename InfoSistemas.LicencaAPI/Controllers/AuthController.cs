using InfoSistemas.LicencaAPI.DTOs;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoSistemas.LicencaAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService svc, IConfiguration cfg) : ControllerBase
{
    // POST /api/auth/login
    // NAO exige X-Admin-Key (e o ponto de entrada da autenticacao)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var resp = await svc.AutenticarAsync(req);
        if (!resp.Sucesso)
            return Unauthorized(resp);
        return Ok(resp);
    }
}
