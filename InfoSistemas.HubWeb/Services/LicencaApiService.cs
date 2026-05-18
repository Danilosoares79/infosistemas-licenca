using System.Net.Http.Json;
using InfoSistemas.HubWeb.DTOs;

namespace InfoSistemas.HubWeb.Services;

public class LicencaApiService(HttpClient http)
{
    // ── Clientes ─────────────────────────────────────────────────────
    public Task<List<ClienteResumoDto>?> ListarClientesAsync() =>
        http.GetFromJsonAsync<List<ClienteResumoDto>>("api/admin/clientes");

    public Task<ClienteResumoDto?> ObterClienteAsync(int id) =>
        http.GetFromJsonAsync<ClienteResumoDto>($"api/admin/clientes/{id}");

    public async Task<ClienteResumoDto?> CriarClienteAsync(CriarClienteRequest req)
    {
        var r = await http.PostAsJsonAsync("api/admin/clientes", req);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<ClienteResumoDto>();
    }

    public async Task AtualizarClienteAsync(int id, AtualizarClienteRequest req)
    {
        var r = await http.PutAsJsonAsync($"api/admin/clientes/{id}", req);
        r.EnsureSuccessStatusCode();
    }

    public async Task BloquearAsync(int id, BloquearRequest req)
    {
        var r = await http.PostAsJsonAsync($"api/admin/clientes/{id}/bloquear", req);
        r.EnsureSuccessStatusCode();
    }

    public async Task DesbloquearAsync(int id)
    {
        var r = await http.PostAsJsonAsync($"api/admin/clientes/{id}/desbloquear", new { });
        r.EnsureSuccessStatusCode();
    }

    public async Task<RenovacaoDto?> RenovarAsync(int id, RenovarRequest req)
    {
        var r = await http.PostAsJsonAsync($"api/admin/clientes/{id}/renovar", req);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<RenovacaoDto>();
    }

    public async Task AlterarVencimentoAsync(int id, DateTime novaData, string? responsavel = null)
    {
        var r = await http.PostAsJsonAsync(
            $"api/admin/clientes/{id}/alterar-vencimento",
            new { NovaData = novaData, Responsavel = responsavel });
        r.EnsureSuccessStatusCode();
    }

    public async Task AtualizarRenovacaoAutomaticaAsync(int id, bool ativo)
    {
        var r = await http.PostAsJsonAsync($"api/admin/clientes/{id}/renovacao-automatica", ativo);
        r.EnsureSuccessStatusCode();
    }

    public Task<List<DispositivoDto>?> ListarDispositivosAsync(int id) =>
        http.GetFromJsonAsync<List<DispositivoDto>>($"api/admin/clientes/{id}/dispositivos");

    public async Task RemoverDispositivoAsync(int clienteId, string machineId)
    {
        var r = await http.DeleteAsync($"api/admin/clientes/{clienteId}/dispositivos/{machineId}");
        r.EnsureSuccessStatusCode();
    }

    public async Task<(int Removidos, string? Mensagem)> RemoverExcedentesAsync(
        int clienteId, string tipo, int novoLimite)
    {
        var r = await http.PostAsJsonAsync(
            $"api/admin/clientes/{clienteId}/remover-dispositivos-excedentes",
            new { Tipo = tipo, NovoLimite = novoLimite });
        if (!r.IsSuccessStatusCode) return (0, "Erro ao remover");
        var obj = await r.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var removidos = obj != null && obj.TryGetValue("removidos", out var v)
            ? int.Parse(v?.ToString() ?? "0") : 0;
        return (removidos, null);
    }

    public Task<List<VencimentoAlertaDto>?> VencimentosAsync(int dias = 7) =>
        http.GetFromJsonAsync<List<VencimentoAlertaDto>>($"api/admin/vencimentos?dias={dias}");

    // ── Arquivo .lic ─────────────────────────────────────────────────
    public async Task<(string nomeArquivo, byte[] bytes)?> DownloadLicAsync(int clienteId)
    {
        var r = await http.GetAsync($"api/admin/clientes/{clienteId}/licenca/download");
        if (!r.IsSuccessStatusCode) return null;
        var bytes = await r.Content.ReadAsByteArrayAsync();
        var nome  = r.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                    ?? $"InfoSistemas_{clienteId}.lic";
        return (nome, bytes);
    }

    // ── Pagamentos ───────────────────────────────────────────────────
    public Task<List<PagamentoDto>?> ListarPagamentosAsync(int? clienteId = null) =>
        http.GetFromJsonAsync<List<PagamentoDto>>(
            clienteId.HasValue ? $"api/admin/pagamentos?clienteId={clienteId}" : "api/admin/pagamentos");

    public async Task<PagamentoDto?> RegistrarPagamentoAsync(int clienteId, RegistrarPagamentoRequest req)
    {
        var r = await http.PostAsJsonAsync($"api/admin/pagamentos/{clienteId}", req);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<PagamentoDto>();
    }
}
