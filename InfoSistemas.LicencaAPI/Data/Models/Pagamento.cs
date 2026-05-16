namespace InfoSistemas.LicencaAPI.Data.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
    public string FormaPagamento { get; set; } = "PIX"; // PIX, BOLETO, CARTAO, DINHEIRO, TRANSFERENCIA
    public string? Observacao { get; set; }
    public string? Responsavel { get; set; }
    public int? DiasRenovados { get; set; }      // se o pagamento gerou renovacao
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
