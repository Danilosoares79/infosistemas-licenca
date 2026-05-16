using InfoSistemas.HubWeb.DTOs;

namespace InfoSistemas.HubWeb.Services;

/// <summary>
/// Estado de autenticacao + permissoes do usuario logado.
///
/// Perfis:
///   Admin       - Acesso total (clientes, usuarios, pagamentos, configuracoes)
///   Operador    - Gerencia clientes e pagamentos; NAO gerencia usuarios
///   Visualizador - Somente leitura; nao pode criar, editar ou excluir nada
/// </summary>
public class AuthState
{
    public UsuarioDto? Usuario { get; private set; }
    public bool MustChangePassword { get; private set; }

    public event Action? OnChange;

    public bool IsAuthenticated => Usuario != null;

    // ── Verificacoes de perfil ─────────────────────────────────────
    public bool IsAdmin        => Usuario?.Perfil == "Admin";
    public bool IsOperador     => Usuario?.Perfil == "Operador";
    public bool IsVisualizador => Usuario?.Perfil == "Visualizador";

    // ── Permissoes granulares ──────────────────────────────────────
    /// Pode criar/editar clientes
    public bool CanEditClientes    => IsAdmin || IsOperador;

    /// Pode bloquear/desbloquear clientes
    public bool CanBloquear        => IsAdmin || IsOperador;

    /// Pode renovar licencas
    public bool CanRenovar         => IsAdmin || IsOperador;

    /// Pode registrar pagamentos
    public bool CanRegistrarPagamento => IsAdmin || IsOperador;

    /// Pode baixar arquivo .lic
    public bool CanDownloadLic     => IsAdmin || IsOperador;

    /// Pode gerenciar usuarios (criar, editar, remover)
    public bool CanManageUsuarios  => IsAdmin;

    /// Pode ver o menu de Usuarios
    public bool CanViewUsuarios    => IsAdmin;

    /// Pode ver a pagina de Pagamentos global
    public bool CanViewPagamentos  => IsAdmin || IsOperador;

    // ──────────────────────────────────────────────────────────────
    public void SetUsuario(UsuarioDto usuario, bool trocarSenha)
    {
        Usuario = usuario;
        MustChangePassword = trocarSenha;
        OnChange?.Invoke();
    }

    public void ClearTrocarSenha()
    {
        MustChangePassword = false;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        Usuario = null;
        MustChangePassword = false;
        OnChange?.Invoke();
    }
}
