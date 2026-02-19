-- =============================================
-- Script de création de la base de données PadelDb
-- Approche Database First : ce script est la
-- source de vérité pour le schéma.
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PadelDb')
BEGIN
    CREATE DATABASE [PadelDb];
END
GO

USE [PadelDb];
GO

-- Table des sites
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sites]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Sites] (
        [Id]      INT            IDENTITY(1,1) NOT NULL,
        [Name]    NVARCHAR(100)  NOT NULL,
        [Address] NVARCHAR(250)  NOT NULL,
        CONSTRAINT [PK_Sites] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
