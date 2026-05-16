namespace InfoSistemas.LicencaAPI.DTOs;

// ── Pagamentos ──────────────────────────────────────────────────────
public record RegistrarPagamentoRequest(
    decimal Valor,
    DateTime DataPagamento,
    string FormaPagamento,       // PIX, BOLETO, CARTAO, DINHEIRO, TRANSFERENCIA
    string? Observacao,
    string? Responsavel,
    bool RenovarLicenca,         // true = tambem renova +DiasRenovados dias
    int DiasRenovados            // usado se RenovarLicenca = true
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

// ── Arquivo .lic ─────────────────────────────────────────────────────
public record LicArquivoDto(
    string NomeArquivo,          // ex: "InfoSistemas_Empresa-X.lic"
    string Conteudo              // base64 do arquivo
);
