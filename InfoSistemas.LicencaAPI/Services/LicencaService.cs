using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InfoSistemas.LicencaAPI.Data;
using InfoSistemas.LicencaAPI.Data.Models;
using InfoSistemas.LicencaAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Services;

public class LicencaService(LicencaDbContext db, IConfiguration cfg)
{
    // ── Validar licenca (chamado pelo Desktop/Mobile ao abrir) ───────
    public async Task<ValidarLicencaResponse> ValidarAsync(ValidarLicencaRequest req)
    {
        var licenca = await db.Licencas
            .Include(l => l.Cliente).ThenInclude(c => c!.Dispositivos)
            .FirstOrDefaultAsync(l => l.Chave == req.Chave);

        if (licenca?.Cliente == null)
            return Bloqueado("Chave de licenca invalida.");

        var cliente = licenca.Cliente;
        var diasRestantes  = (int)(cliente.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays;
        var desktopsAtivos = cliente.Dispositivos.Count(d => d.Ativo && d.Tipo == "DESKTOP");
        var mobileAtivos   = cliente.Dispositivos.Count(d => d.Ativo && d.Tipo == "MOBILE");

        if (cliente.Status == "BLOQUEADO")
            return Bloqueado($"Sistema bloqueado. {cliente.MotivoBloqueio}");

        if (cliente.Status == "EXPIRADO" || diasRestantes < 0)
        {
            cliente.Status = "EXPIRADO";
            await db.SaveChangesAsync();
            return Bloqueado("Licenca expirada. Contate a InfoSistemas para renovar.");
        }

        var dispositivoExistente = cliente.Dispositivos.FirstOrDefault(d => d.MachineId == req.MachineId);
        if (dispositivoExistente == null)
        {
            var limite = req.Tipo == "DESKTOP" ? cliente.MaxDesktops : cliente.MaxMobile;
            var ativos  = req.Tipo == "DESKTOP" ? desktopsAtivos : mobileAtivos;
            if (ativos >= limite)
                return Bloqueado($"Limite atingido ({limite}/{limite}). Contate a InfoSistemas.");

            db.Dispositivos.Add(new Dispositivo
            {
                ClienteId  = cliente.Id,
                MachineId  = req.MachineId,
                Tipo       = req.Tipo,
                Nome       = req.Tipo == "DESKTOP" ? $"Desktop {desktopsAtivos+1}" : $"Mobile {mobileAtivos+1}",
                AppVersion = req.AppVersion
            });
            await db.SaveChangesAsync();
        }
        else
        {
            dispositivoExistente.UltimoAcesso = DateTime.UtcNow;
            dispositivoExistente.AppVersion   = req.AppVersion;
            await db.SaveChangesAsync();
        }

        var alertar  = diasRestantes <= 5 && diasRestantes >= 0;
        var mensagem = alertar ? $"ATENCAO: Sua licenca expira em {diasRestantes} dia(s)." : "Licenca valida.";

        return new ValidarLicencaResponse(
            Valido: true, Status: "ATIVO",
            DiasRestantes: diasRestantes, Alertar: alertar, Mensagem: mensagem,
            RazaoSocial: cliente.RazaoSocial, Fantasia: cliente.Fantasia,
            Cnpj: cliente.Cnpj, Email: cliente.Email, Fone: cliente.Fone,
            Endereco: cliente.Endereco, Cidade: cliente.Cidade, Uf: cliente.Uf,
            MaxDesktops: cliente.MaxDesktops, DesktopsAtivos: desktopsAtivos,
            MaxMobile: cliente.MaxMobile, MobileAtivos: mobileAtivos
        );
    }

    // ── Heartbeat ────────────────────────────────────────────────────
    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest req)
    {
        var licenca = await db.Licencas
            .Include(l => l.Cliente).ThenInclude(c => c!.Dispositivos)
            .FirstOrDefaultAsync(l => l.Chave == req.Chave);

        if (licenca?.Cliente == null)
            return new HeartbeatResponse(false, "INVALIDO", 0, false, "Licenca invalida.");

        var cliente       = licenca.Cliente;
        var diasRestantes = (int)(cliente.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays;

        var dispositivo = cliente.Dispositivos.FirstOrDefault(d => d.MachineId == req.MachineId);
        if (dispositivo != null) { dispositivo.UltimoAcesso = DateTime.UtcNow; await db.SaveChangesAsync(); }

        if (cliente.Status == "BLOQUEADO")
            return new HeartbeatResponse(false, "BLOQUEADO", diasRestantes, false, $"Sistema bloqueado. {cliente.MotivoBloqueio}");

        if (cliente.Status == "EXPIRADO" || diasRestantes < 0)
        {
            if (cliente.Status != "EXPIRADO") { cliente.Status = "EXPIRADO"; await db.SaveChangesAsync(); }
            return new HeartbeatResponse(false, "EXPIRADO", diasRestantes, false, "Licenca expirada.");
        }

        var alertar  = diasRestantes <= 5;
        return new HeartbeatResponse(true, "ATIVO", diasRestantes, alertar,
            alertar ? $"ATENCAO: Licenca expira em {diasRestantes} dia(s)!" : "OK");
    }

    // ── CRUD Clientes ─────────────────────────────────────────────────
    public async Task<List<ClienteResumoDto>> ListarClientesAsync()
    {
        var clientes = await db.Clientes
            .Include(c => c.Licenca).Include(c => c.Dispositivos)
            .OrderBy(c => c.RazaoSocial).ToListAsync();
        return clientes.Select(ToResumo).ToList();
    }

    public async Task<ClienteResumoDto?> ObterClienteAsync(int id)
    {
        var c = await db.Clientes
            .Include(c => c.Licenca).Include(c => c.Dispositivos)
            .FirstOrDefaultAsync(c => c.Id == id);
        return c == null ? null : ToResumo(c);
    }

    public async Task<ClienteResumoDto> CriarClienteAsync(CriarClienteRequest req)
    {
        var cliente = new Cliente
        {
            RazaoSocial = req.RazaoSocial, Fantasia = req.Fantasia,
            Cnpj = req.Cnpj, Email = req.Email, Fone = req.Fone,
            Endereco = req.Endereco, Cidade = req.Cidade, Uf = req.Uf, Cep = req.Cep,
            Plano = req.Plano, MaxDesktops = req.MaxDesktops, MaxMobile = req.MaxMobile,
            DataVencimento = req.DataVencimento, Status = "ATIVO",
            Licenca = new Licenca { Chave = GerarChave() }
        };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        await db.Entry(cliente).Reference(c => c.Licenca).LoadAsync();
        return ToResumo(cliente);
    }

    public async Task AtualizarClienteAsync(int id, AtualizarClienteRequest req)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new Exception("Cliente nao encontrado.");
        c.RazaoSocial = req.RazaoSocial; c.Fantasia = req.Fantasia;
        c.Email = req.Email; c.Fone = req.Fone; c.Endereco = req.Endereco;
        c.Cidade = req.Cidade; c.Uf = req.Uf; c.Cep = req.Cep;
        c.Plano = req.Plano; c.MaxDesktops = req.MaxDesktops; c.MaxMobile = req.MaxMobile;
        c.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task BloquearAsync(int id, BloquearRequest req)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new Exception("Cliente nao encontrado.");
        c.Status = "BLOQUEADO"; c.MotivoBloqueio = $"{req.Motivo} (por {req.Responsavel})";
        c.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DesbloquearAsync(int id)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new Exception("Cliente nao encontrado.");
        c.Status = "ATIVO"; c.MotivoBloqueio = null; c.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<RenovacaoDto> RenovarAsync(int id, RenovarRequest req)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new Exception("Cliente nao encontrado.");
        var dataAnterior = c.DataVencimento;
        var base_ = c.DataVencimento < DateTime.UtcNow ? DateTime.UtcNow : c.DataVencimento;
        var dataNova = base_.AddDays(req.Dias);
        c.DataVencimento = dataNova; c.Status = "ATIVO"; c.MotivoBloqueio = null;
        c.AtualizadoEm = DateTime.UtcNow;
        var r = new Renovacao { ClienteId = id, DataAnterior = dataAnterior, DataNova = dataNova,
            DiasAdicionados = req.Dias, Responsavel = req.Responsavel, Observacao = req.Observacao };
        db.Renovacoes.Add(r);
        await db.SaveChangesAsync();
        return new RenovacaoDto(r.Id, dataAnterior, dataNova, req.Dias, req.Responsavel, req.Observacao, r.CriadaEm);
    }

    public async Task AtualizarRenovacaoAutomaticaAsync(int id, bool ativo)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new Exception("Cliente nao encontrado.");
        c.RenovacaoAutomatica = ativo;
        await db.SaveChangesAsync();
    }

    public async Task<List<DispositivoDto>> ListarDispositivosAsync(int clienteId)
    {
        var lista = await db.Dispositivos.Where(d => d.ClienteId == clienteId)
            .OrderByDescending(d => d.UltimoAcesso).ToListAsync();
        return lista.Select(d => new DispositivoDto(d.Id, d.MachineId, d.Tipo, d.Nome, d.AppVersion, d.UltimoAcesso, d.Ativo)).ToList();
    }

    public async Task RemoverDispositivoAsync(int clienteId, string machineId)
    {
        var d = await db.Dispositivos.FirstOrDefaultAsync(d => d.ClienteId == clienteId && d.MachineId == machineId)
            ?? throw new Exception("Dispositivo nao encontrado.");
        d.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<VencimentoAlertaDto>> VencimentosProximosAsync(int dias = 7)
    {
        var limite = DateTime.UtcNow.AddDays(dias);
        var clientes = await db.Clientes
            .Where(c => c.Status == "ATIVO" && c.DataVencimento <= limite)
            .OrderBy(c => c.DataVencimento).ToListAsync();
        return clientes.Select(c => new VencimentoAlertaDto(
            c.Id, c.RazaoSocial, c.Email, c.Fone, c.DataVencimento,
            (int)(c.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays)).ToList();
    }

    // ── PAGAMENTOS ───────────────────────────────────────────────────
    public async Task<List<PagamentoDto>> ListarPagamentosAsync(int? clienteId = null)
    {
        var q = db.Pagamentos.Include(p => p.Cliente).AsQueryable();
        if (clienteId.HasValue) q = q.Where(p => p.ClienteId == clienteId.Value);
        var lista = await q.OrderByDescending(p => p.DataPagamento).Take(200).ToListAsync();
        return lista.Select(p => new PagamentoDto(
            p.Id, p.ClienteId, p.Cliente?.RazaoSocial ?? "",
            p.Valor, p.DataPagamento, p.FormaPagamento,
            p.Observacao, p.Responsavel, p.DiasRenovados, p.CriadoEm)).ToList();
    }

    public async Task<PagamentoDto> RegistrarPagamentoAsync(int clienteId, RegistrarPagamentoRequest req)
    {
        var cliente = await db.Clientes.FindAsync(clienteId) ?? throw new Exception("Cliente nao encontrado.");

        var pagamento = new Pagamento
        {
            ClienteId      = clienteId,
            Valor          = req.Valor,
            DataPagamento  = req.DataPagamento,
            FormaPagamento = req.FormaPagamento,
            Observacao     = req.Observacao,
            Responsavel    = req.Responsavel,
            DiasRenovados  = req.RenovarLicenca ? req.DiasRenovados : null,
            CriadoEm       = DateTime.UtcNow
        };

        db.Pagamentos.Add(pagamento);

        // Se marcado pra renovar, renova automaticamente
        if (req.RenovarLicenca && req.DiasRenovados > 0)
        {
            var base_ = cliente.DataVencimento < DateTime.UtcNow ? DateTime.UtcNow : cliente.DataVencimento;
            var dataAnterior = cliente.DataVencimento;
            cliente.DataVencimento = base_.AddDays(req.DiasRenovados);
            cliente.Status = "ATIVO";
            cliente.MotivoBloqueio = null;
            cliente.AtualizadoEm = DateTime.UtcNow;

            db.Renovacoes.Add(new Renovacao
            {
                ClienteId = clienteId, DataAnterior = dataAnterior,
                DataNova = cliente.DataVencimento, DiasAdicionados = req.DiasRenovados,
                Responsavel = req.Responsavel ?? "Pagamento",
                Observacao = $"Renovacao via pagamento de R$ {req.Valor:F2}"
            });
        }

        await db.SaveChangesAsync();
        return new PagamentoDto(
            pagamento.Id, clienteId, cliente.RazaoSocial,
            pagamento.Valor, pagamento.DataPagamento, pagamento.FormaPagamento,
            pagamento.Observacao, pagamento.Responsavel, pagamento.DiasRenovados, pagamento.CriadoEm);
    }

    // ── ARQUIVO .LIC (HMAC-SHA256 assinado) ──────────────────────────
    public async Task<LicArquivoDto> GerarArquivoLicAsync(int clienteId)
    {
        var cliente = await db.Clientes
            .Include(c => c.Licenca)
            .FirstOrDefaultAsync(c => c.Id == clienteId)
            ?? throw new Exception("Cliente nao encontrado.");

        // Dados que o PDV vai verificar offline
        var payload = new
        {
            chave           = cliente.Licenca?.Chave ?? "",
            razaoSocial     = cliente.RazaoSocial,
            cnpj            = cliente.Cnpj,
            plano           = cliente.Plano,
            maxDesktops     = cliente.MaxDesktops,
            maxMobile       = cliente.MaxMobile,
            dataVencimento  = cliente.DataVencimento.ToString("yyyy-MM-dd"),
            emitidoEm       = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            versao          = "2"
        };

        var json       = JsonSerializer.Serialize(payload);
        var jsonBytes  = Encoding.UTF8.GetBytes(json);
        var secretKey  = cfg["LicSecretKey"] ?? "INFOSISTEMAS-LIC-SECRET-2026";
        var keyBytes   = Encoding.UTF8.GetBytes(secretKey);

        // Gera assinatura HMAC-SHA256
        var hmac      = HMACSHA256.HashData(keyBytes, jsonBytes);
        var assinatura = Convert.ToBase64String(hmac);

        // Envelope final: base64(json) + "." + assinatura
        var envelope = Convert.ToBase64String(jsonBytes) + "." + assinatura;

        // Nome do arquivo: InfoSistemas_Empresa-X.lic
        var nomeCliente = new string(cliente.RazaoSocial
            .Replace(" ", "-").Replace("/", "").Replace("\\", "")
            .Where(c2 => char.IsLetterOrDigit(c2) || c2 == '-' || c2 == '_').ToArray());
        var nomeArquivo = $"InfoSistemas_{nomeCliente}.lic";

        return new LicArquivoDto(nomeArquivo, envelope);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private static string GerarChave()
    {
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd   = new Random();
        string Seg() => new(Enumerable.Range(0, 4).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        return $"{Seg()}-{Seg()}-{Seg()}-{Seg()}";
    }

    private static ClienteResumoDto ToResumo(Cliente c) => new(
        c.Id, c.RazaoSocial, c.Fantasia, c.Cnpj, c.Email, c.Status, c.Plano,
        c.MaxDesktops, c.MaxMobile,
        c.Dispositivos.Count(d => d.Ativo && d.Tipo == "DESKTOP"),
        c.Dispositivos.Count(d => d.Ativo && d.Tipo == "MOBILE"),
        c.DataVencimento,
        (int)(c.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays,
        c.Licenca?.Chave ?? ""
    );

    private static ValidarLicencaResponse Bloqueado(string msg) =>
        new(false, "BLOQUEADO", 0, false, msg, "", "", "", "", "", "", "", "", 0, 0, 0, 0);
}
