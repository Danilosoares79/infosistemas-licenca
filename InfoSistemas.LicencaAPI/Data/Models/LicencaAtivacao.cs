namespace InfoSistemas.LicencaAPI.Data.Models;

/// <summary>
/// Registra a ativacao de um arquivo .lic.
/// Cada download do .lic gera um token unico.
/// O token so pode ser usado para ativar 1 computador (MachineId).
/// Isso impede que o mesmo .lic seja instalado em multiplos PCs.
/// </summary>
public class LicencaAtivacao
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>GUID unico gerado no download do .lic</summary>
    public string ActivationToken { get; set; } = "";

    /// <summary>MachineId do PC que ativou o token (null = ainda nao ativado)</summary>
    public string? MachineId { get; set; }

    /// <summary>Quando o token foi ativado pela primeira vez (null = nunca)</summary>
    public DateTime? ActivadoEm { get; set; }

    /// <summary>Quando o arquivo .lic foi gerado (download)</summary>
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public bool Ativado => MachineId != null;
}
