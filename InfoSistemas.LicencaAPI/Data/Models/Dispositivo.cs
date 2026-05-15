namespace InfoSistemas.LicencaAPI.Data.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string MachineId { get; set; } = string.Empty; // hash único da máquina
    public string Tipo { get; set; } = "DESKTOP"; // DESKTOP, MOBILE
    public string Nome { get; set; } = string.Empty; // ex: "Caixa 1", "Garcom Joao"
    public string AppVersion { get; set; } = string.Empty;
    public DateTime UltimoAcesso { get; set; } = DateTime.UtcNow;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;

    public Cliente? Cliente { get; set; }
}
