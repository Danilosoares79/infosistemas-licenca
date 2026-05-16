using InfoSistemas.HubWeb.DTOs;

namespace InfoSistemas.HubWeb.Services;

/// <summary>
/// Estado de autenticacao do usuario no circuit Blazor.
/// Singleton por sessao (scoped) - cada aba mantem seu proprio estado.
/// </summary>
public class AuthState
{
    public UsuarioDto? Usuario { get; private set; }
    public bool MustChangePassword { get; private set; }

    public event Action? OnChange;

    public bool IsAuthenticated => Usuario != null;
    public bool IsAdmin => Usuario?.Perfil == "Admin";

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
