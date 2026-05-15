namespace InfoSistemas.LicencaAPI.Data.Models;

public class Licenca
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Chave { get; set; } = string.Empty; // XXXX-XXXX-XXXX-XXXX
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public Cliente? Cliente { get; set; }
}
