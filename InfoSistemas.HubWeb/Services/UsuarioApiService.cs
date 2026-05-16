using System.Net.Http.Json;
using InfoSistemas.HubWeb.DTOs;

namespace InfoSistemas.HubWeb.Services;

public class UsuarioApiService(HttpClient http)
{
    public Task<List<UsuarioDto>?> ListarAsync() =>
        http.GetFromJsonAsync<List<UsuarioDto>>("api/admin/usuarios");

    public Task<UsuarioDto?> ObterAsync(int id) =>
        http.GetFromJsonAsync<UsuarioDto>($"api/admin/usuarios/{id}");

    public async Task<(bool Sucesso, string? Erro, UsuarioDto? Usuario)> CriarAsync(CriarUsuarioRequest req)
    {
        var r = await http.PostAsJsonAsync("api/admin/usuarios", req);
        if (r.IsSuccessStatusCode)
        {
            var u = await r.Content.ReadFromJsonAsync<UsuarioDto>();
            return (true, null, u);
        }
        var erro = await TentarLerErroAsync(r);
        return (false, erro, null);
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarAsync(int id, AtualizarUsuarioRequest req)
    {
        var r = await http.PutAsJsonAsync($"api/admin/usuarios/{id}", req);
        if (r.IsSuccessStatusCode) return (true, null);
        return (false, await TentarLerErroAsync(r));
    }

    public async Task<(bool Sucesso, string? Erro)> AlterarSenhaAsync(int id, AlterarSenhaRequest req)
    {
        var r = await http.PostAsJsonAsync($"api/admin/usuarios/{id}/alterar-senha", req);
        if (r.IsSuccessStatusCode) return (true, null);
        return (false, await TentarLerErroAsync(r));
    }

    public async Task<(bool Sucesso, string? Erro)> ResetarSenhaAsync(int id, ResetarSenhaRequest req)
    {
        var r = await http.PostAsJsonAsync($"api/admin/usuarios/{id}/resetar-senha", req);
        if (r.IsSuccessStatusCode) return (true, null);
        return (false, await TentarLerErroAsync(r));
    }

    public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
    {
        var r = await http.DeleteAsync($"api/admin/usuarios/{id}");
        if (r.IsSuccessStatusCode) return (true, null);
        return (false, await TentarLerErroAsync(r));
    }

    private static async Task<string?> TentarLerErroAsync(HttpResponseMessage r)
    {
        try
        {
            var obj = await r.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
            if (obj != null && obj.TryGetValue("erro", out var v) && !string.IsNullOrEmpty(v))
                return v;
        }
        catch { }
        return $"Erro do servidor: {(int)r.StatusCode} {r.ReasonPhrase}";
    }
}
