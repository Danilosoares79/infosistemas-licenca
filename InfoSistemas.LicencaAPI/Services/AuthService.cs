using InfoSistemas.LicencaAPI.Data;
using InfoSistemas.LicencaAPI.Data.Models;
using InfoSistemas.LicencaAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace InfoSistemas.LicencaAPI.Services;

public class AuthService(LicencaDbContext db)
{
    public async Task<LoginResponse> AutenticarAsync(LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Senha))
            return new LoginResponse(false, "Login e senha sao obrigatorios", null, false);

        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Login == req.Login);
        if (u == null)
            return new LoginResponse(false, "Login ou senha invalidos", null, false);

        if (!u.Ativo)
            return new LoginResponse(false, "Usuario inativo. Contate o administrador.", null, false);

        bool ok;
        try { ok = BC.Verify(req.Senha, u.SenhaHash); }
        catch { ok = false; }

        if (!ok)
            return new LoginResponse(false, "Login ou senha invalidos", null, false);

        u.UltimoAcesso = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var dto = ToDto(u);
        return new LoginResponse(true, null, dto, u.TrocarSenhaProximoLogin);
    }

    public async Task<List<UsuarioDto>> ListarAsync()
    {
        var lista = await db.Usuarios.OrderBy(u => u.Login).ToListAsync();
        return lista.Select(ToDto).ToList();
    }

    public async Task<UsuarioDto?> ObterAsync(int id)
    {
        var u = await db.Usuarios.FindAsync(id);
        return u == null ? null : ToDto(u);
    }

    public async Task<(bool Sucesso, string? Erro, UsuarioDto? Usuario)> CriarAsync(CriarUsuarioRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Senha) || string.IsNullOrWhiteSpace(req.Nome))
            return (false, "Login, senha e nome sao obrigatorios", null);

        if (req.Senha.Length < 4)
            return (false, "Senha deve ter no minimo 4 caracteres", null);

        var existe = await db.Usuarios.AnyAsync(u => u.Login == req.Login);
        if (existe)
            return (false, "Ja existe um usuario com esse login", null);

        var u = new Usuario
        {
            Login = req.Login.Trim(),
            SenhaHash = BC.HashPassword(req.Senha),
            Nome = req.Nome.Trim(),
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
            Perfil = string.IsNullOrWhiteSpace(req.Perfil) ? "Admin" : req.Perfil,
            Ativo = true,
            TrocarSenhaProximoLogin = false,
            DataCriacao = DateTime.UtcNow
        };

        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        return (true, null, ToDto(u));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarAsync(int id, AtualizarUsuarioRequest req)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u == null) return (false, "Usuario nao encontrado");

        u.Nome = req.Nome.Trim();
        u.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        u.Perfil = req.Perfil;
        u.Ativo = req.Ativo;

        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> AlterarSenhaAsync(int id, AlterarSenhaRequest req)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u == null) return (false, "Usuario nao encontrado");

        bool ok;
        try { ok = BC.Verify(req.SenhaAtual, u.SenhaHash); }
        catch { ok = false; }

        if (!ok) return (false, "Senha atual incorreta");

        if (string.IsNullOrWhiteSpace(req.NovaSenha) || req.NovaSenha.Length < 4)
            return (false, "Nova senha deve ter no minimo 4 caracteres");

        u.SenhaHash = BC.HashPassword(req.NovaSenha);
        u.TrocarSenhaProximoLogin = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> ResetarSenhaAsync(int id, ResetarSenhaRequest req)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u == null) return (false, "Usuario nao encontrado");

        if (string.IsNullOrWhiteSpace(req.NovaSenha) || req.NovaSenha.Length < 4)
            return (false, "Nova senha deve ter no minimo 4 caracteres");

        u.SenhaHash = BC.HashPassword(req.NovaSenha);
        u.TrocarSenhaProximoLogin = true;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u == null) return (false, "Usuario nao encontrado");

        // Nao permitir remover o ultimo admin
        var totalAdmins = await db.Usuarios.CountAsync(x => x.Perfil == "Admin" && x.Ativo);
        if (u.Perfil == "Admin" && u.Ativo && totalAdmins <= 1)
            return (false, "Nao e possivel remover o ultimo administrador ativo");

        db.Usuarios.Remove(u);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task SeedAdminPadraoAsync()
    {
        var existe = await db.Usuarios.AnyAsync();
        if (existe) return; // ja tem usuarios, nao cria

        var admin = new Usuario
        {
            Login = "admin",
            SenhaHash = BC.HashPassword("admin"),
            Nome = "Administrador",
            Email = null,
            Perfil = "Admin",
            Ativo = true,
            TrocarSenhaProximoLogin = true, // forca troca no primeiro login
            DataCriacao = DateTime.UtcNow
        };

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
    }

    private static UsuarioDto ToDto(Usuario u) => new(
        u.Id, u.Login, u.Nome, u.Email, u.Perfil, u.Ativo, u.UltimoAcesso, u.DataCriacao
    );
}
