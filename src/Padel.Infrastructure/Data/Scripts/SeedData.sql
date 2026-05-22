-- =============================================
-- Script d'insertion de données de démonstration
-- Padel Manager — Présentation
-- =============================================

USE [PadelDb];
GO

-- Nettoyage des données existantes (dans l'ordre inverse des FK)
DELETE FROM [dbo].[Payments];
DELETE FROM [dbo].[MatchPlayers];
DELETE FROM [dbo].[Matches];
DELETE FROM [dbo].[Members];
DELETE FROM [dbo].[ClosureDays];
DELETE FROM [dbo].[SiteSchedules];
DELETE FROM [dbo].[Courts];
DELETE FROM [dbo].[Sites];
GO

-- =============================================
-- 1. SITES
-- =============================================

INSERT INTO [dbo].[Sites] ([Name], [Address]) VALUES
('Padel Center Bruxelles', '123 Avenue Louise, 1050 Bruxelles'),
('Padel Club Liège', '45 Rue Saint-Laurent, 4000 Liège'),
('Padel Arena Namur', '78 Boulevard de la Meuse, 5000 Namur');
GO

-- =============================================
-- 2. TERRAINS
-- =============================================

-- Site 1 : Bruxelles (3 terrains)
INSERT INTO [dbo].[Courts] ([Name], [SiteId]) VALUES
('Terrain Central', 1),
('Terrain Nord', 1),
('Terrain Sud', 1);

-- Site 2 : Liège (2 terrains)
INSERT INTO [dbo].[Courts] ([Name], [SiteId]) VALUES
('Court Principal', 2),
('Court Annexe', 2);

-- Site 3 : Namur (2 terrains)
INSERT INTO [dbo].[Courts] ([Name], [SiteId]) VALUES
('Arena 1', 3),
('Arena 2', 3);
GO

-- =============================================
-- 3. HORAIRES (année en cours)
-- =============================================

DECLARE @CurrentYear INT = YEAR(GETDATE());

INSERT INTO [dbo].[SiteSchedules] ([SiteId], [Year], [StartTime], [EndTime]) VALUES
(1, @CurrentYear, '08:00:00', '22:00:00'),  -- Bruxelles : 8h-22h
(2, @CurrentYear, '09:00:00', '21:00:00'),  -- Liège : 9h-21h
(3, @CurrentYear, '08:00:00', '23:00:00');  -- Namur : 8h-23h
GO

-- =============================================
-- 4. JOURS DE FERMETURE
-- =============================================

-- Fermetures globales (tous les sites)
INSERT INTO [dbo].[ClosureDays] ([Date], [Reason], [SiteId]) VALUES
('2025-01-01', 'Nouvel An', NULL),
('2025-12-25', 'Noël', NULL);

-- Fermetures spécifiques
INSERT INTO [dbo].[ClosureDays] ([Date], [Reason], [SiteId]) VALUES
('2025-03-15', 'Maintenance annuelle', 1),  -- Bruxelles
('2025-06-20', 'Événement privé', 2);       -- Liège
GO

-- =============================================
-- 5. MEMBRES
-- =============================================

-- Membres Global (Gxxxx) - peuvent réserver partout, 21 jours à l'avance
INSERT INTO [dbo].[Members] ([Matricule], [FirstName], [LastName], [Email], [MemberType], [SiteId], [ReservationBlocked], [BlockedUntil]) VALUES
('G0001', 'Jean', 'Dupont', 'jean.dupont@email.com', 'Global', NULL, 0, NULL),
('G0002', 'Marie', 'Martin', 'marie.martin@email.com', 'Global', NULL, 0, NULL),
('G0003', 'Pierre', 'Bernard', 'pierre.bernard@email.com', 'Global', NULL, 0, NULL),
('G0004', 'Sophie', 'Dubois', 'sophie.dubois@email.com', 'Global', NULL, 0, NULL);

-- Membres Site (Sxxxxx) - réservent sur leur site uniquement, 14 jours à l'avance
INSERT INTO [dbo].[Members] ([Matricule], [FirstName], [LastName], [Email], [MemberType], [SiteId], [ReservationBlocked], [BlockedUntil]) VALUES
('S00001', 'Luc', 'Leroy', 'luc.leroy@email.com', 'Site', 1, 0, NULL),        -- Bruxelles
('S00002', 'Anne', 'Moreau', 'anne.moreau@email.com', 'Site', 1, 0, NULL),    -- Bruxelles
('S00003', 'Thomas', 'Simon', 'thomas.simon@email.com', 'Site', 2, 0, NULL),  -- Liège
('S00004', 'Julie', 'Laurent', 'julie.laurent@email.com', 'Site', 2, 0, NULL), -- Liège
('S00005', 'Marc', 'Petit', 'marc.petit@email.com', 'Site', 3, 0, NULL);      -- Namur

-- Membres Libre (Lxxxxx) - peuvent réserver partout, 5 jours à l'avance
INSERT INTO [dbo].[Members] ([Matricule], [FirstName], [LastName], [Email], [MemberType], [SiteId], [ReservationBlocked], [BlockedUntil]) VALUES
('L00001', 'Paul', 'Roux', 'paul.roux@email.com', 'Libre', NULL, 0, NULL),
('L00002', 'Emma', 'Garcia', 'emma.garcia@email.com', 'Libre', NULL, 0, NULL),
('L00003', 'Lucas', 'Robert', 'lucas.robert@email.com', 'Libre', NULL, 0, NULL);

-- Un membre bloqué (pour démonstration)
INSERT INTO [dbo].[Members] ([Matricule], [FirstName], [LastName], [Email], [MemberType], [SiteId], [ReservationBlocked], [BlockedUntil]) VALUES
('G0005', 'Alex', 'Bloqué', 'alex.bloque@email.com', 'Global', NULL, 1, DATEADD(DAY, 5, GETDATE()));
GO

-- =============================================
-- 6. MATCHS
-- =============================================

-- Match 1 : Privé complet (Full) demain 10h-11h30 à Bruxelles - Tous payés
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(1, 1, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('10:00:00' AS DATETIME)), 
      DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('11:30:00' AS DATETIME)), 
      'Private', 'Full');

-- Match 2 : Public avec 2 joueurs (Scheduled) après-demain 14h-15h30 à Bruxelles
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(1, 3, DATEADD(DAY, 2, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('14:00:00' AS DATETIME)), 
      DATEADD(DAY, 2, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('15:30:00' AS DATETIME)), 
      'Public', 'Scheduled');

-- Match 3 : Public complet (Full) dans 3 jours 16h-17h30 à Liège
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(4, 7, DATEADD(DAY, 3, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('16:00:00' AS DATETIME)), 
      DATEADD(DAY, 3, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('17:30:00' AS DATETIME)), 
      'Public', 'Full');

-- Match 4 : Privé incomplet (2 joueurs) dans 4 jours 18h-19h30 à Namur
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(6, 9, DATEADD(DAY, 4, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('18:00:00' AS DATETIME)), 
      DATEADD(DAY, 4, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('19:30:00' AS DATETIME)), 
      'Private', 'Scheduled');

-- Match 5 : Public avec 3 joueurs dans 5 jours 10h-11h30 à Bruxelles
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(2, 5, DATEADD(DAY, 5, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('10:00:00' AS DATETIME)), 
      DATEADD(DAY, 5, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('11:30:00' AS DATETIME)), 
      'Public', 'Scheduled');

-- Match 6 : Privé avec 3 joueurs dans 6 jours 20h-21h30 à Liège
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(5, 8, DATEADD(DAY, 6, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('20:00:00' AS DATETIME)), 
      DATEADD(DAY, 6, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('21:30:00' AS DATETIME)), 
      'Private', 'Scheduled');

-- Match 7 : Passé et complété (il y a 2 jours) - pour les statistiques
INSERT INTO [dbo].[Matches] ([CourtId], [OrganizerId], [ScheduledAt], [EndsAt], [MatchType], [Status]) VALUES
(3, 2, DATEADD(DAY, -2, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('10:00:00' AS DATETIME)), 
      DATEADD(DAY, -2, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST('11:30:00' AS DATETIME)), 
      'Public', 'Completed');
GO

-- =============================================
-- 7. JOUEURS INSCRITS AUX MATCHS (MatchPlayers)
-- =============================================

-- Match 1 : 4 joueurs (organisateur + 3 autres)
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(1, 1, DATEADD(DAY, -10, GETDATE())),  -- G0001 (organisateur)
(1, 2, DATEADD(DAY, -10, GETDATE())),  -- G0002
(1, 3, DATEADD(DAY, -9, GETDATE())),   -- G0003
(1, 4, DATEADD(DAY, -8, GETDATE()));   -- G0004

-- Match 2 : 2 joueurs
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(2, 3, DATEADD(DAY, -7, GETDATE())),   -- G0003 (organisateur)
(2, 11, DATEADD(DAY, -6, GETDATE()));  -- L00001

-- Match 3 : 4 joueurs
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(3, 7, DATEADD(DAY, -5, GETDATE())),   -- S00003 (organisateur)
(3, 8, DATEADD(DAY, -5, GETDATE())),   -- S00004
(3, 12, DATEADD(DAY, -4, GETDATE())),  -- L00002
(3, 13, DATEADD(DAY, -3, GETDATE()));  -- L00003

-- Match 4 : 2 joueurs (privé incomplet)
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(4, 9, DATEADD(DAY, -4, GETDATE())),   -- S00005 (organisateur)
(4, 4, DATEADD(DAY, -3, GETDATE()));   -- G0004

-- Match 5 : 3 joueurs
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(5, 5, DATEADD(DAY, -3, GETDATE())),   -- S00001 (organisateur)
(5, 6, DATEADD(DAY, -2, GETDATE())),   -- S00002
(5, 2, DATEADD(DAY, -2, GETDATE()));   -- G0002

-- Match 6 : 3 joueurs
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(6, 8, DATEADD(DAY, -2, GETDATE())),   -- S00004 (organisateur)
(6, 1, DATEADD(DAY, -1, GETDATE())),   -- G0001
(6, 11, DATEADD(DAY, -1, GETDATE()));  -- L00001

-- Match 7 : 4 joueurs (match complété)
INSERT INTO [dbo].[MatchPlayers] ([MatchId], [MemberId], [JoinedAt]) VALUES
(7, 2, DATEADD(DAY, -15, GETDATE())),  -- G0002 (organisateur)
(7, 5, DATEADD(DAY, -15, GETDATE())),  -- S00001
(7, 6, DATEADD(DAY, -14, GETDATE())),  -- S00002
(7, 12, DATEADD(DAY, -13, GETDATE())); -- L00002
GO

-- =============================================
-- 8. PAIEMENTS
-- =============================================

-- Match 1 : Tous payés (15€ chacun)
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(1, 1, 1, 15.00, 'Paid', DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -9, GETDATE())),
(2, 1, 2, 15.00, 'Paid', DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -9, GETDATE())),
(3, 1, 3, 15.00, 'Paid', DATEADD(DAY, -9, GETDATE()), DATEADD(DAY, -8, GETDATE())),
(4, 1, 4, 15.00, 'Paid', DATEADD(DAY, -8, GETDATE()), DATEADD(DAY, -7, GETDATE()));

-- Match 2 : 1 payé, 1 en attente
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(5, 2, 3, 15.00, 'Paid', DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -6, GETDATE())),
(6, 2, 11, 15.00, 'Pending', DATEADD(DAY, -6, GETDATE()), NULL);

-- Match 3 : Tous payés
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(7, 3, 7, 15.00, 'Paid', DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -4, GETDATE())),
(8, 3, 8, 15.00, 'Paid', DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -4, GETDATE())),
(9, 3, 12, 15.00, 'Paid', DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -3, GETDATE())),
(10, 3, 13, 15.00, 'Paid', DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -2, GETDATE()));

-- Match 4 : 1 payé, 1 en attente (privé incomplet)
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(11, 4, 9, 15.00, 'Paid', DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -3, GETDATE())),
(12, 4, 4, 15.00, 'Pending', DATEADD(DAY, -3, GETDATE()), NULL);

-- Match 5 : 2 payés, 1 en attente
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(13, 5, 5, 15.00, 'Paid', DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -2, GETDATE())),
(14, 5, 6, 15.00, 'Paid', DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -1, GETDATE())),
(15, 5, 2, 15.00, 'Pending', DATEADD(DAY, -2, GETDATE()), NULL);

-- Match 6 : Tous en attente (risque retrait J-1)
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(16, 6, 8, 15.00, 'Pending', DATEADD(DAY, -2, GETDATE()), NULL),
(17, 6, 1, 15.00, 'Pending', DATEADD(DAY, -1, GETDATE()), NULL),
(18, 6, 11, 15.00, 'Pending', DATEADD(DAY, -1, GETDATE()), NULL);

-- Match 7 : Tous payés (match complété)
INSERT INTO [dbo].[Payments] ([MatchPlayerId], [MatchId], [MemberId], [Amount], [Status], [CreatedAt], [PaidAt]) VALUES
(19, 7, 2, 15.00, 'Paid', DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -14, GETDATE())),
(20, 7, 5, 15.00, 'Paid', DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -14, GETDATE())),
(21, 7, 6, 15.00, 'Paid', DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -13, GETDATE())),
(22, 7, 12, 15.00, 'Paid', DATEADD(DAY, -13, GETDATE()), DATEADD(DAY, -12, GETDATE()));
GO

-- =============================================
-- Vérifications et résumé
-- =============================================

PRINT '=============================================';
PRINT 'Insertion des données de démonstration terminée';
PRINT '=============================================';
PRINT '';
PRINT 'Résumé des données insérées :';
PRINT '---------------------------------------------';
PRINT 'Sites : ' + CAST((SELECT COUNT(*) FROM Sites) AS VARCHAR);
PRINT 'Terrains : ' + CAST((SELECT COUNT(*) FROM Courts) AS VARCHAR);
PRINT 'Horaires : ' + CAST((SELECT COUNT(*) FROM SiteSchedules) AS VARCHAR);
PRINT 'Jours de fermeture : ' + CAST((SELECT COUNT(*) FROM ClosureDays) AS VARCHAR);
PRINT 'Membres : ' + CAST((SELECT COUNT(*) FROM Members) AS VARCHAR);
PRINT '  - Global : ' + CAST((SELECT COUNT(*) FROM Members WHERE MemberType = 'Global') AS VARCHAR);
PRINT '  - Site : ' + CAST((SELECT COUNT(*) FROM Members WHERE MemberType = 'Site') AS VARCHAR);
PRINT '  - Libre : ' + CAST((SELECT COUNT(*) FROM Members WHERE MemberType = 'Libre') AS VARCHAR);
PRINT 'Matchs : ' + CAST((SELECT COUNT(*) FROM Matches) AS VARCHAR);
PRINT '  - Privés : ' + CAST((SELECT COUNT(*) FROM Matches WHERE MatchType = 'Private') AS VARCHAR);
PRINT '  - Publics : ' + CAST((SELECT COUNT(*) FROM Matches WHERE MatchType = 'Public') AS VARCHAR);
PRINT 'Joueurs inscrits : ' + CAST((SELECT COUNT(*) FROM MatchPlayers) AS VARCHAR);
PRINT 'Paiements : ' + CAST((SELECT COUNT(*) FROM Payments) AS VARCHAR);
PRINT '  - Payés : ' + CAST((SELECT COUNT(*) FROM Payments WHERE Status = 'Paid') AS VARCHAR);
PRINT '  - En attente : ' + CAST((SELECT COUNT(*) FROM Payments WHERE Status = 'Pending') AS VARCHAR);
PRINT '';
PRINT 'Chiffre d''affaires total : ' + CAST((SELECT SUM(Amount) FROM Payments WHERE Status = 'Paid') AS VARCHAR) + ' €';
PRINT '=============================================';
GO
