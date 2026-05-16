-- Script para adicionar novas funcionalidades ao banco
-- Execute no MySQL do cPanel via phpMyAdmin ou pelo servidor

-- 1. Adicionar coluna RenovacaoAutomatica na tabela Clientes
ALTER TABLE Clientes 
ADD COLUMN IF NOT EXISTS RenovacaoAutomatica TINYINT(1) NOT NULL DEFAULT 0;

-- 2. Criar tabela de Pagamentos
CREATE TABLE IF NOT EXISTS Pagamentos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ClienteId INT NOT NULL,
    Valor DECIMAL(10,2) NOT NULL,
    DataPagamento DATETIME(6) NOT NULL,
    FormaPagamento VARCHAR(20) NOT NULL DEFAULT 'PIX',
    Observacao VARCHAR(255) NULL,
    Responsavel VARCHAR(80) NULL,
    DiasRenovados INT NULL,
    CriadoEm DATETIME(6) NOT NULL,
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

-- Verificar resultado
SHOW COLUMNS FROM Clientes LIKE 'RenovacaoAutomatica';
SHOW TABLES LIKE 'Pagamentos';

SELECT 'Script executado com sucesso!' AS Resultado;
