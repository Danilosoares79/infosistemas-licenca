namespace InfoSistemas.LicencaAPI.Data.Models;

public class Renovacao
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataAnterior { get; set; }
    public DateTime DataNova { get; set; }
    public int DiasAdicionados { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public Cliente? Cliente { get; set; }
}
