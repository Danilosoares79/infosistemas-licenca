namespace InfoSistemas.HubWeb.DTOs;

public record ValidarLicencaRequest(string Chave, string MachineId, string Tipo, string AppVersion);
public record HeartbeatRequest(string Chave, string MachineId);

public record CriarClienteRequest(
    string RazaoSocial, string Fantasia, string Cnpj, string Email, string Fone,
    string Endereco, string Cidade, string Uf, string Cep,
    string Plano, int MaxDesktops, int MaxMobile, DateTime DataVencimento);

public record AtualizarClienteRequest(
    string RazaoSocial, string Fantasia, string Email, string Fone,
    string Endereco, string Cidade, string Uf, string Cep,
    string Plano, int MaxDesktops, int MaxMobile);

public record RenovarRequest(int Dias, string Responsavel, string Observacao);
public record BloquearRequest(string Motivo, string Responsavel);

public record ClienteResumoDto(
    int Id, string RazaoSocial, string Fantasia, string Cnpj, string Email,
    string Status, string Plano, int MaxDesktops, int MaxMobile,
    int DesktopsAtivos, int MobileAtivos,
    DateTime DataVencimento, int DiasRestantes, string Chave,
    string? MotivoBloqueio = null);

public record DispositivoDto(
    int Id, string MachineId, string Tipo, string Nome,
    string AppVersion, DateTime UltimoAcesso, bool Ativo);

public record RenovacaoDto(
    int Id, DateTime DataAnterior, DateTime DataNova,
    int DiasAdicionados, string Responsavel, string Observacao, DateTime CriadaEm);

public record VencimentoAlertaDto(
    int ClienteId, string RazaoSocial, string Email,
    string Fone, DateTime DataVencimento, int DiasRestantes);

// ── Pagamentos ─────────────────────────────────────────────────────────────
public record RegistrarPagamentoRequest(
    decimal Valor,
    DateTime DataPagamento,
    string FormaPagamento,
    string? Observacao,
    string? Responsavel,
    bool RenovarLicenca,
    int DiasRenovados
);

public record PagamentoDto(
    int Id,
    int ClienteId,
    string RazaoSocial,
    decimal Valor,
    DateTime DataPagamento,
    string FormaPagamento,
    string? Observacao,
    string? Responsavel,
    int? DiasRenovados,
    DateTime CriadoEm
);
