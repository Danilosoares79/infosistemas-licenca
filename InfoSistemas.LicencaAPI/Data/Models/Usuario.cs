namespace InfoSistemas.LicencaAPI.Data.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Nome { get; set; } = "";
    public string? Email { get; set; }
    public string Perfil { get; set; } = "Admin";  // futuro: Admin, Operador, Visualizador
    public bool Ativo { get; set; } = true;
    public bool TrocarSenhaProximoLogin { get; set; } = false;
    public DateTime? UltimoAcesso { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
