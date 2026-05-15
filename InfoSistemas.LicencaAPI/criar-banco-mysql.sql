-- ================================================================
--  InfoSistemas PDV -- Sistema de Licenciamento
--  Banco: cardapi6_infosistemas_licenca
--  Execute no phpMyAdmin: selecione o banco > aba SQL > colar > Executar
-- ================================================================

SET NAMES utf8mb4;
SET time_zone = '-03:00';
SET foreign_key_checks = 0;

-- ----------------------------------------------------------------
-- Remove tabelas antigas se existirem (ordem importa por FK)
-- ----------------------------------------------------------------
DROP TABLE IF EXISTS `Renovacoes`;
DROP TABLE IF EXISTS `Dispositivos`;
DROP TABLE IF EXISTS `Licencas`;
DROP TABLE IF EXISTS `Clientes`;

-- ----------------------------------------------------------------
-- Tabela: Clientes
-- Armazena todos os clientes do sistema de licenciamento
-- ----------------------------------------------------------------
CREATE TABLE `Clientes` (
  `Id`             INT           NOT NULL AUTO_INCREMENT,
  `RazaoSocial`    VARCHAR(100)  NOT NULL,
  `Fantasia`       VARCHAR(100)  NOT NULL DEFAULT '',
  `Cnpj`           VARCHAR(18)   NOT NULL DEFAULT '',
  `Email`          VARCHAR(100)  NOT NULL DEFAULT '',
  `Fone`           VARCHAR(20)   NOT NULL DEFAULT '',
  `Endereco`       VARCHAR(150)  NOT NULL DEFAULT '',
  `Cidade`         VARCHAR(80)   NOT NULL DEFAULT '',
  `Uf`             VARCHAR(2)    NOT NULL DEFAULT '',
  `Cep`            VARCHAR(10)   NOT NULL DEFAULT '',
  `Plano`          VARCHAR(20)   NOT NULL DEFAULT 'BASICO',
  `MaxDesktops`    INT           NOT NULL DEFAULT 1,
  `MaxMobile`      INT           NOT NULL DEFAULT 0,
  `DataVencimento` DATETIME      NOT NULL,
  `Status`         VARCHAR(20)   NOT NULL DEFAULT 'ATIVO',
  `MotivoBloqueio` VARCHAR(255)      NULL DEFAULT NULL,
  `CriadoEm`      DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `AtualizadoEm`  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                          ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `idx_status`     (`Status`),
  INDEX `idx_vencimento` (`DataVencimento`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Clientes do sistema de licenciamento InfoSistemas';

-- ----------------------------------------------------------------
-- Tabela: Licencas
-- Cada cliente tem exatamente uma licenca com chave unica
-- ----------------------------------------------------------------
CREATE TABLE `Licencas` (
  `Id`        INT         NOT NULL AUTO_INCREMENT,
  `ClienteId` INT         NOT NULL,
  `Chave`     VARCHAR(20) NOT NULL COMMENT 'Formato: XXXX-XXXX-XXXX-XXXX',
  `CriadaEm` DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_chave`   (`Chave`),
  UNIQUE KEY `uq_cliente` (`ClienteId`),
  CONSTRAINT `fk_licenca_cliente`
    FOREIGN KEY (`ClienteId`)
    REFERENCES `Clientes` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Chaves de licenca dos clientes';

-- ----------------------------------------------------------------
-- Tabela: Dispositivos
-- Registra cada computador/celular que ativou o sistema
-- ----------------------------------------------------------------
CREATE TABLE `Dispositivos` (
  `Id`           INT         NOT NULL AUTO_INCREMENT,
  `ClienteId`    INT         NOT NULL,
  `MachineId`    VARCHAR(64) NOT NULL COMMENT 'Hash SHA256 da maquina',
  `Tipo`         VARCHAR(10) NOT NULL DEFAULT 'DESKTOP'
                             COMMENT 'DESKTOP ou MOBILE',
  `Nome`         VARCHAR(80) NOT NULL DEFAULT ''
                             COMMENT 'Ex: Caixa 1, Garcom Joao',
  `AppVersion`   VARCHAR(20) NOT NULL DEFAULT '',
  `UltimoAcesso` DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
                             ON UPDATE CURRENT_TIMESTAMP,
  `CriadoEm`    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Ativo`        TINYINT(1)  NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_cliente_machine` (`ClienteId`, `MachineId`),
  INDEX `idx_tipo`  (`Tipo`),
  INDEX `idx_ativo` (`Ativo`),
  CONSTRAINT `fk_disp_cliente`
    FOREIGN KEY (`ClienteId`)
    REFERENCES `Clientes` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Dispositivos (desktops e celulares) registrados por cliente';

-- ----------------------------------------------------------------
-- Tabela: Renovacoes
-- Historico de todas as renovacoes de licenca
-- ----------------------------------------------------------------
CREATE TABLE `Renovacoes` (
  `Id`              INT          NOT NULL AUTO_INCREMENT,
  `ClienteId`       INT          NOT NULL,
  `DataAnterior`    DATETIME     NOT NULL COMMENT 'Vencimento antes da renovacao',
  `DataNova`        DATETIME     NOT NULL COMMENT 'Novo vencimento apos renovacao',
  `DiasAdicionados` INT          NOT NULL DEFAULT 30,
  `Responsavel`     VARCHAR(80)  NOT NULL DEFAULT ''
                                 COMMENT 'Quem fez a renovacao',
  `Observacao`      VARCHAR(255) NOT NULL DEFAULT '',
  `CriadaEm`       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `idx_cliente` (`ClienteId`),
  INDEX `idx_criada`  (`CriadaEm`),
  CONSTRAINT `fk_renov_cliente`
    FOREIGN KEY (`ClienteId`)
    REFERENCES `Clientes` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Historico de renovacoes de licenca';

-- ----------------------------------------------------------------
SET foreign_key_checks = 1;

-- ----------------------------------------------------------------
-- Cliente de teste para validar o sistema
-- Chave: TEST-1234-INFO-SIST
-- ----------------------------------------------------------------
INSERT INTO `Clientes`
  (`RazaoSocial`, `Fantasia`, `Cnpj`, `Email`, `Fone`,
   `Endereco`, `Cidade`, `Uf`, `Cep`,
   `Plano`, `MaxDesktops`, `MaxMobile`,
   `DataVencimento`, `Status`)
VALUES (
  'InfoSistemas Teste Ltda',
  'Teste PDV',
  '00.000.000/0001-00',
  'teste@infosistemas.net.br',
  '(71) 99999-9999',
  'Rua Teste, 123',
  'Salvador',
  'BA',
  '41000-000',
  'PROFISSIONAL',
  3,
  5,
  DATE_ADD(NOW(), INTERVAL 30 DAY),
  'ATIVO'
);

INSERT INTO `Licencas` (`ClienteId`, `Chave`)
VALUES (LAST_INSERT_ID(), 'TEST-1234-INFO-SIST');

-- ----------------------------------------------------------------
-- Verificacao final: mostra resumo do que foi criado
-- ----------------------------------------------------------------
SELECT 'Clientes'    AS Tabela, COUNT(*) AS Registros FROM `Clientes`
UNION ALL
SELECT 'Licencas',              COUNT(*)               FROM `Licencas`
UNION ALL
SELECT 'Dispositivos',          COUNT(*)               FROM `Dispositivos`
UNION ALL
SELECT 'Renovacoes',            COUNT(*)               FROM `Renovacoes`;

SELECT
  c.Id,
  c.RazaoSocial,
  c.Plano,
  c.MaxDesktops,
  c.MaxMobile,
  DATE_FORMAT(c.DataVencimento, '%d/%m/%Y') AS Vencimento,
  c.Status,
  l.Chave                                   AS ChaveLicenca
FROM `Clientes`  c
JOIN `Licencas`  l ON l.ClienteId = c.Id;

-- ================================================================
--  CONCLUIDO!
--  4 tabelas criadas: Clientes, Licencas, Dispositivos, Renovacoes
--  Chave de teste: TEST-1234-INFO-SIST
-- ================================================================
