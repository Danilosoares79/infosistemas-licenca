namespace InfoSistemas.LicencaAPI.DTOs;

// ── Ativação do .lic (chamado pelo PDV na primeira instalação) ────────
public record AtivarLicencaRequest(
    string ActivationToken,  // do arquivo .lic
    string MachineId,        // ID do computador sendo instalado
    string Tipo,             // DESKTOP ou MOBILE
    string AppVersion        // versao do PDV
);

public record AtivarLicencaResponse(
    bool Sucesso,
    string Mensagem,
    // dados retornados para o PDV configurar o sistema
    string? Chave,
    string? RazaoSocial,
    string? Fantasia,
    string? Cnpj,
    string? Plano,
    int MaxDesktops,
    int MaxMobile,
    string? DataVencimento
);
