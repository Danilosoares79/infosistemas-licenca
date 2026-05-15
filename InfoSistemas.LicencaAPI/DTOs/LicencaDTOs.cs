namespace InfoSistemas.LicencaAPI.DTOs;

// ── Requisições do Desktop/Mobile ────────────────────────────
public record ValidarLicencaRequest(
    string Chave,
    string MachineId,
    string Tipo,       // DESKTOP | MOBILE
    string AppVersion
);

public record HeartbeatRequest(
    string Chave,
    string MachineId
);

public record RegistrarDispositivoRequest(
    string Chave,
    string MachineId,
    string Tipo,       // DESKTOP | MOBILE
    string Nome        // ex: "Caixa 1", "Garcom Joao"
);

// ── Respostas para Desktop/Mobile ────────────────────────────
public record ValidarLicencaResponse(
    bool   Valido,
    string Status,          // ATIVO | BLOQUEADO | EXPIRADO
    int    DiasRestantes,
    bool   Alertar,
    string Mensagem,
    string RazaoSocial,
    string Fantasia,
    string Cnpj,
    string Email,
    string Fone,
    string Endereco,
    string Cidade,
    string Uf,
    int    MaxDesktops,
    int    DesktopsAtivos,
    int    MaxMobile,
    int    MobileAtivos
);

public record HeartbeatResponse(
    bool   Valido,
    string Status,
    int    DiasRestantes,
    bool   Alertar,
    string Mensagem
);

public record RegistrarDispositivoResponse(
    bool   Autorizado,
    string Mensagem,
    int    QtdAtual,
    int    QtdMax
);

// ── Requisições do HubWeb (Admin) ────────────────────────────
public record CriarClienteRequest(
    string RazaoSocial,
    string Fantasia,
    string Cnpj,
    string Email,
    string Fone,
    string Endereco,
    string Cidade,
    string Uf,
    string Cep,
    string Plano,
    int    MaxDesktops,
    int    MaxMobile,
    DateTime DataVencimento
);

public record AtualizarClienteRequest(
    string RazaoSocial,
    string Fantasia,
    string Email,
    string Fone,
    string Endereco,
    string Cidade,
    string Uf,
    string Cep,
    string Plano,
    int    MaxDesktops,
    int    MaxMobile
);

public record RenovarRequest(
    int    Dias,
    string Responsavel,
    string Observacao
);

public record BloquearRequest(
    string Motivo,
    string Responsavel
);

// ── Respostas do HubWeb ───────────────────────────────────────
public record ClienteResumoDto(
    int      Id,
    string   RazaoSocial,
    string   Fantasia,
    string   Cnpj,
    string   Email,
    string   Status,
    string   Plano,
    int      MaxDesktops,
    int      MaxMobile,
    int      DesktopsAtivos,
    int      MobileAtivos,
    DateTime DataVencimento,
    int      DiasRestantes,
    string   Chave
);

public record DispositivoDto(
    int      Id,
    string   MachineId,
    string   Tipo,
    string   Nome,
    string   AppVersion,
    DateTime UltimoAcesso,
    bool     Ativo
);

public record RenovacaoDto(
    int      Id,
    DateTime DataAnterior,
    DateTime DataNova,
    int      DiasAdicionados,
    string   Responsavel,
    string   Observacao,
    DateTime CriadaEm
);

public record VencimentoAlertaDto(
    int      ClienteId,
    string   RazaoSocial,
    string   Email,
    string   Fone,
    DateTime DataVencimento,
    int      DiasRestantes
);
