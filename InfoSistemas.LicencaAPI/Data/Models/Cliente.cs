namespace InfoSistemas.LicencaAPI.Data.Models;

public class Cliente
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Fantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Fone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Plano { get; set; } = "BASICO";
    public int MaxDesktops { get; set; } = 1;
    public int MaxMobile { get; set; } = 0;
    public DateTime DataVencimento { get; set; } = DateTime.UtcNow.AddDays(30);
    public string Status { get; set; } = "ATIVO";
    public string? MotivoBloqueio { get; set; }
    public bool RenovacaoAutomatica { get; set; } = false; // renova +30 dias ao expirar
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Navegação
    public Licenca? Licenca { get; set; }
    public List<Dispositivo> Dispositivos { get; set; } = [];
    public List<Renovacao> Renovacoes { get; set; } = [];
    public List<Pagamento> Pagamentos { get; set; } = [];
}
