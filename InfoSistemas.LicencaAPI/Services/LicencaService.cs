using InfoSistemas.LicencaAPI.Data;
using InfoSistemas.LicencaAPI.Data.Models;
using InfoSistemas.LicencaAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InfoSistemas.LicencaAPI.Services;

public class LicencaService(LicencaDbContext db)
{
    // ── Validar licenca (chamado pelo Desktop/Mobile ao abrir) ───────
    public async Task<ValidarLicencaResponse> ValidarAsync(ValidarLicencaRequest req)
    {
        var licenca = await db.Licencas
            .Include(l => l.Cliente)
            .ThenInclude(c => c!.Dispositivos)
            .FirstOrDefaultAsync(l => l.Chave == req.Chave);

        if (licenca?.Cliente == null)
            return Bloqueado("Chave de licenca invalida.");

        var cliente = licenca.Cliente;
        var diasRestantes = (int)(cliente.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays;
        var desktopsAtivos = cliente.Dispositivos.Count(d => d.Ativo && d.Tipo == "DESKTOP");
        var mobileAtivos   = cliente.Dispositivos.Count(d => d.Ativo && d.Tipo == "MOBILE");

        // Verificar status do cliente
        if (cliente.Status == "BLOQUEADO")
            return Bloqueado($"Sistema bloqueado. {cliente.MotivoBloqueio}");

        if (cliente.Status == "EXPIRADO" || diasRestantes < 0)
        {
            cliente.Status = "EXPIRADO";
            await db.SaveChangesAsync();
            return Bloqueado("Licenca expirada. Contate a InfoSistemas para renovar.");
        }

        // Verificar limite de dispositivos
        var dispositivoExistente = cliente.Dispositivos.FirstOrDefault(d => d.MachineId == req.MachineId);
        if (dispositivoExistente == null)
        {
            var limite = req.Tipo == "DESKTOP" ? cliente.MaxDesktops : cliente.MaxMobile;
            var ativos  = req.Tipo == "DESKTOP" ? desktopsAtivos : mobileAtivos;
            if (ativos >= limite)
                return Bloqueado($"Limite de {(req.Tipo == "DESKTOP" ? "desktops" : "dispositivos mobile")} atingido ({limite}/{limite}). Contate a InfoSistemas.");

            // Registrar novo dispositivo
            db.Dispositivos.Add(new Dispositivo
            {
                ClienteId  = cliente.Id,
                MachineId  = req.MachineId,
                Tipo       = req.Tipo,
                Nome       = req.Tipo == "DESKTOP" ? $"Desktop {desktopsAtivos + 1}" : $"Mobile {mobileAtivos + 1}",
                AppVersion = req.AppVersion
            });
            await db.SaveChangesAsync();
        }
        else
        {
            // Atualizar ultimo acesso
            dispositivoExistente.UltimoAcesso = DateTime.UtcNow;
            dispositivoExistente.AppVersion   = req.AppVersion;
            await db.SaveChangesAsync();
        }

        var alertar  = diasRestantes <= 5 && diasRestantes >= 0;
        var mensagem = alertar
            ? $"ATENCAO: Sua licenca expira em {diasRestantes} dia(s). Contate a InfoSistemas para renovar."
            : "Licenca valida.";

        return new ValidarLicencaResponse(
            Valido: true, Status: "ATIVO",
            DiasRestantes: diasRestantes, Alertar: alertar, Mensagem: mensagem,
            RazaoSocial: cliente.RazaoSocial, Fantasia: cliente.Fantasia,
            Cnpj: cliente.Cnpj, Email: cliente.Email, Fone: cliente.Fone,
            Endereco: cliente.Endereco, Cidade: cliente.Cidade, Uf: cliente.Uf,
            MaxDesktops: cliente.MaxDesktops, DesktopsAtivos: desktopsAtivos,
            MaxMobile: cliente.MaxMobile,   MobileAtivos: mobileAtivos
        );
    }

    // ── Heartbeat (chamado a cada 15 min pelo Desktop/Mobile) ───────
    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest req)
    {
        var licenca = await db.Licencas
            .Include(l => l.Cliente)
            .ThenInclude(c => c!.Dispositivos)
            .FirstOrDefaultAsync(l => l.Chave == req.Chave);

        if (licenca?.Cliente == null)
            return new HeartbeatResponse(false, "INVALIDO", 0, false, "Licenca invalida.");

        var cliente = licenca.Cliente;
        var diasRestantes = (int)(cliente.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays;

        // Atualizar ultimo acesso do dispositivo
        var dispositivo = cliente.Dispositivos.FirstOrDefault(d => d.MachineId == req.MachineId);
        if (dispositivo != null)
        {
            dispositivo.UltimoAcesso = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        if (cliente.Status == "BLOQUEADO")
            return new HeartbeatResponse(false, "BLOQUEADO", diasRestantes, false,
                $"Sistema bloqueado. {cliente.MotivoBloqueio}");

        if (cliente.Status == "EXPIRADO" || diasRestantes < 0)
        {
            if (cliente.Status != "EXPIRADO") { cliente.Status = "EXPIRADO"; await db.SaveChangesAsync(); }
            return new HeartbeatResponse(false, "EXPIRADO", diasRestantes, false,
                "Licenca expirada. Contate a InfoSistemas.");
        }

        var alertar  = diasRestantes <= 5;
        var mensagem = alertar
            ? $"ATENCAO: Licenca expira em {diasRestantes} dia(s)!"
            : "OK";

        return new HeartbeatResponse(true, "ATIVO", diasRestantes, alertar, mensagem);
    }

    // ── CRUD Clientes (HubWeb Admin) ─────────────────────────────────
    public async Task<List<ClienteResumoDto>> ListarClientesAsync()
    {
        var clientes = await db.Clientes
            .Include(c => c.Licenca)
            .Include(c => c.Dispositivos)
            .OrderBy(c => c.RazaoSocial)
            .ToListAsync();

        return clientes.Select(ToResumo).ToList();
    }

    public async Task<ClienteResumoDto?> ObterClienteAsync(int id)
    {
        var c = await db.Clientes
            .Include(c => c.Licenca)
            .Include(c => c.Dispositivos)
            .FirstOrDefaultAsync(c => c.Id == id);
        return c == null ? null : ToResumo(c);
    }

    public async Task<ClienteResumoDto> CriarClienteAsync(CriarClienteRequest req)
    {
        var chave = GerarChave();
        var cliente = new Cliente
        {
            RazaoSocial     = req.RazaoSocial,
            Fantasia        = req.Fantasia,
            Cnpj            = req.Cnpj,
            Email           = req.Email,
            Fone            = req.Fone,
            Endereco        = req.Endereco,
            Cidade          = req.Cidade,
            Uf              = req.Uf,
            Cep             = req.Cep,
            Plano           = req.Plano,
            MaxDesktops     = req.MaxDesktops,
            MaxMobile       = req.MaxMobile,
            DataVencimento  = req.DataVencimento,
            Status          = "ATIVO",
            Licenca         = new Licenca { Chave = chave }
        };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        await db.Entry(cliente).Reference(c => c.Licenca).LoadAsync();
        return ToResumo(cliente);
    }

    public async Task AtualizarClienteAsync(int id, AtualizarClienteRequest req)
    {
        var cliente = await db.Clientes.FindAsync(id)
            ?? throw new Exception("Cliente nao encontrado.");
        cliente.RazaoSocial = req.RazaoSocial;
        cliente.Fantasia    = req.Fantasia;
        cliente.Email       = req.Email;
        cliente.Fone        = req.Fone;
        cliente.Endereco    = req.Endereco;
        cliente.Cidade      = req.Cidade;
        cliente.Uf          = req.Uf;
        cliente.Cep         = req.Cep;
        cliente.Plano       = req.Plano;
        cliente.MaxDesktops = req.MaxDesktops;
        cliente.MaxMobile   = req.MaxMobile;
        cliente.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task BloquearAsync(int id, BloquearRequest req)
    {
        var cliente = await db.Clientes.FindAsync(id)
            ?? throw new Exception("Cliente nao encontrado.");
        cliente.Status         = "BLOQUEADO";
        cliente.MotivoBloqueio = $"{req.Motivo} (por {req.Responsavel})";
        cliente.AtualizadoEm   = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DesbloquearAsync(int id)
    {
        var cliente = await db.Clientes.FindAsync(id)
            ?? throw new Exception("Cliente nao encontrado.");
        cliente.Status         = "ATIVO";
        cliente.MotivoBloqueio = null;
        cliente.AtualizadoEm   = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<RenovacaoDto> RenovarAsync(int id, RenovarRequest req)
    {
        var cliente = await db.Clientes.FindAsync(id)
            ?? throw new Exception("Cliente nao encontrado.");

        var dataAnterior = cliente.DataVencimento;
        // Renova a partir do vencimento atual (mesmo que ja tenha passado)
        var base_ = cliente.DataVencimento < DateTime.UtcNow
            ? DateTime.UtcNow
            : cliente.DataVencimento;
        var dataNova = base_.AddDays(req.Dias);

        cliente.DataVencimento = dataNova;
        cliente.Status         = "ATIVO";
        cliente.MotivoBloqueio = null;
        cliente.AtualizadoEm   = DateTime.UtcNow;

        var renovacao = new Renovacao
        {
            ClienteId      = id,
            DataAnterior   = dataAnterior,
            DataNova       = dataNova,
            DiasAdicionados = req.Dias,
            Responsavel    = req.Responsavel,
            Observacao     = req.Observacao
        };
        db.Renovacoes.Add(renovacao);
        await db.SaveChangesAsync();

        return new RenovacaoDto(renovacao.Id, dataAnterior, dataNova, req.Dias,
            req.Responsavel, req.Observacao, renovacao.CriadaEm);
    }

    public async Task<List<DispositivoDto>> ListarDispositivosAsync(int clienteId)
    {
        var lista = await db.Dispositivos
            .Where(d => d.ClienteId == clienteId)
            .OrderByDescending(d => d.UltimoAcesso)
            .ToListAsync();

        return lista.Select(d => new DispositivoDto(
            d.Id, d.MachineId, d.Tipo, d.Nome,
            d.AppVersion, d.UltimoAcesso, d.Ativo)).ToList();
    }

    public async Task RemoverDispositivoAsync(int clienteId, string machineId)
    {
        var d = await db.Dispositivos
            .FirstOrDefaultAsync(d => d.ClienteId == clienteId && d.MachineId == machineId)
            ?? throw new Exception("Dispositivo nao encontrado.");
        d.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<VencimentoAlertaDto>> VencimentosProximosAsync(int dias = 7)
    {
        var limite = DateTime.UtcNow.AddDays(dias);
        var clientes = await db.Clientes
            .Where(c => c.Status == "ATIVO" && c.DataVencimento <= limite)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        return clientes.Select(c => new VencimentoAlertaDto(
            c.Id, c.RazaoSocial, c.Email, c.Fone, c.DataVencimento,
            (int)(c.DataVencimento.Date - DateTime.UtcNow.Date).TotalDays
        )).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────
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
