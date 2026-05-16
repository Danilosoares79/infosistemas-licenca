namespace InfoSistemas.HubWeb.DTOs;

public record LoginRequest(string Login, string Senha);

public record LoginResponse(
    bool Sucesso,
    string? Mensagem,
    UsuarioDto? Usuario,
    bool TrocarSenha
);

public record UsuarioDto(
    int Id,
    string Login,
    string Nome,
    string? Email,
    string Perfil,
    bool Ativo,
    DateTime? UltimoAcesso,
    DateTime DataCriacao
);

public record CriarUsuarioRequest(
    string Login,
    string Senha,
    string Nome,
    string? Email,
    string Perfil = "Admin"
);

public record AtualizarUsuarioRequest(
    string Nome,
    string? Email,
    string Perfil,
    bool Ativo
);

public record AlterarSenhaRequest(
    string SenhaAtual,
    string NovaSenha
);

public record ResetarSenhaRequest(
    string NovaSenha
);
