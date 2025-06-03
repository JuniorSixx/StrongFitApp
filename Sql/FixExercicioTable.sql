-- Verificar se a coluna Series existe e adicioná-la se não existir
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Series')
BEGIN
    ALTER TABLE [dbo].[Exercicios]
    ADD [Series] INT NOT NULL DEFAULT 3;
END

-- Verificar se a coluna Repeticoes existe e adicioná-la se não existir
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Repeticoes')
BEGIN
    ALTER TABLE [dbo].[Exercicios]
    ADD [Repeticoes] INT NOT NULL DEFAULT 12;
END
