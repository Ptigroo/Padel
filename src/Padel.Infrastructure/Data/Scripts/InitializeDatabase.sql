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

-- Table des terrains
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Courts]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Courts] (
        [Id]     INT           IDENTITY(1,1) NOT NULL,
        [Name]   NVARCHAR(100) NOT NULL,
        [SiteId] INT           NOT NULL,
        CONSTRAINT [PK_Courts] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Courts_Sites] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Sites]([Id])
    );
END
GO

-- Table des horaires par site et par année
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteSchedules]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[SiteSchedules] (
        [Id]        INT      IDENTITY(1,1) NOT NULL,
        [SiteId]    INT      NOT NULL,
        [Year]      INT      NOT NULL,
        [StartTime] TIME(7)  NOT NULL,
        [EndTime]   TIME(7)  NOT NULL,
        CONSTRAINT [PK_SiteSchedules] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SiteSchedules_Sites] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Sites]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_SiteSchedules_SiteId_Year] UNIQUE ([SiteId], [Year])
    );
END
GO

-- Table des jours de fermeture
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClosureDays]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[ClosureDays] (
        [Id]     INT            IDENTITY(1,1) NOT NULL,
        [Date]   DATE           NOT NULL,
        [Reason] NVARCHAR(250)  NULL,
        [SiteId] INT            NULL,
        CONSTRAINT [PK_ClosureDays] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ClosureDays_Sites] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Sites]([Id]) ON DELETE CASCADE
    );
END
GO

-- Table des membres
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Members]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Members] (
        [Id]                 INT            IDENTITY(1,1) NOT NULL,
        [Matricule]          NVARCHAR(10)   NOT NULL,
        [FirstName]          NVARCHAR(100)  NOT NULL,
        [LastName]           NVARCHAR(100)  NOT NULL,
        [Email]              NVARCHAR(200)  NOT NULL,
        [MemberType]         NVARCHAR(10)   NOT NULL,
        [SiteId]             INT            NULL,
        [ReservationBlocked] BIT            NOT NULL DEFAULT 0,
        [BlockedUntil]       DATETIME2      NULL,
        CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Members_Sites] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Sites]([Id]) ON DELETE SET NULL,
        CONSTRAINT [UQ_Members_Matricule] UNIQUE ([Matricule])
    );
END
GO
