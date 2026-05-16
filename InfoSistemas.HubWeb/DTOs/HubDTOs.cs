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
    string? MotivoBloqueio);

public record DispositivoDto(
    int Id, string MachineId, string Tipo, string Nome,
    string AppVersion, DateTime UltimoAcesso, bool Ativo);

public record RenovacaoDto(
    int Id, DateTime DataAnterior, DateTime DataNova,
    int DiasAdicionados, string Responsavel, string Observacao, DateTime CriadaEm);

public record VencimentoAlertaDto(
    int ClienteId, string RazaoSocial, string Email,
    string Fone, DateTime DataVencimento, int DiasRestantes);

// ============================================================================
// CLASSES ADICIONADAS PARA CORRIGIR ERRO CS1061 E CS0649
// Data: 15/05/2026 - Deploy Railway v6.0
// ============================================================================

/// <summary>
/// DTO para dados de release de cliente
/// </summary>
public class ClientReleaseData
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    
    // PROPRIEDADES ADICIONADAS PARA CORRIGIR O ERRO CS1061:
    public int? MetodologiaId { get; set; }
    public virtual Metodologia? Metodologia { get; set; }
    
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Metodologia relacionada aos releases
/// </summary>
public class Metodologia
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}

/// <summary>
/// Modelo para gerenciamento de Hotsite
/// </summary>
public class Hotsite
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    
    // CORRECAO DO WARNING CS0649:
    // Transformado de campo para propriedade com inicializacao
    public List<ClientData> clients_full { get; set; } = new List<ClientData>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Dados de cliente para Hotsite
/// </summary>
public class ClientData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
