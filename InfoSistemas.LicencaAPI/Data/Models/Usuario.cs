namespace InfoSistemas.LicencaAPI.Data.Models;

/// <summary>
/// Perfis de usuario do sistema de licenciamento.
/// Admin: acesso total
/// Operador: gerencia clientes e pagamentos, nao gerencia usuarios
/// Visualizador: somente leitura
/// </summary>
public static class Perfis
{
    public const string Admin       = "Admin";
    public const string Operador    = "Operador";
    public const string Visualizador = "Visualizador";

    public static readonly string[] Todos = [Admin, Operador, Visualizador];
}

public class Usuario
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Nome { get; set; } = "";
    public string? Email { get; set; }
    public string Perfil { get; set; } = Perfis.Admin;
    public bool Ativo { get; set; } = true;
    public bool TrocarSenhaProximoLogin { get; set; } = false;
    public DateTime? UltimoAcesso { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
