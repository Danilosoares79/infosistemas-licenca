using System.Net.Http.Json;
using InfoSistemas.HubWeb.DTOs;

namespace InfoSistemas.HubWeb.Services;

public class AuthApiService(HttpClient http)
{
    public async Task<LoginResponse> LoginAsync(string login, string senha)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("api/auth/login", new LoginRequest(login, senha));
            // Tanto 200 OK quanto 401 retornam LoginResponse no body
            var result = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            return result ?? new LoginResponse(false, "Resposta invalida do servidor", null, false);
        }
        catch (Exception ex)
        {
            return new LoginResponse(false, $"Erro ao conectar com o servidor: {ex.Message}", null, false);
        }
    }
}
