-- Adicionar tabela de ativacoes do arquivo .lic
-- Cada download do .lic gera 1 token unico
-- O token so pode ativar 1 MachineId (computador)

CREATE TABLE IF NOT EXISTS LicencaAtivacoes (
    Id               INT AUTO_INCREMENT PRIMARY KEY,
    ClienteId        INT NOT NULL,
    ActivationToken  VARCHAR(36) NOT NULL,
    MachineId        VARCHAR(64) NULL,
    ActivadoEm       DATETIME(6) NULL,
    CriadoEm         DATETIME(6) NOT NULL,
    UNIQUE KEY UQ_ActivationToken (ActivationToken),
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

SELECT 'Tabela LicencaAtivacoes criada com sucesso!' AS Resultado;
SHOW TABLES LIKE 'LicencaAtivacoes';
