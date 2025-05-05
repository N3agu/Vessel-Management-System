IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Owners] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Owners] PRIMARY KEY ([Id])
);

CREATE TABLE [Ships] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [ImoNumber] nvarchar(7) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Tonnage] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Ships] PRIMARY KEY ([Id])
);

CREATE TABLE [ShipOwners] (
    [OwnerId] int NOT NULL,
    [ShipId] int NOT NULL,
    CONSTRAINT [PK_ShipOwners] PRIMARY KEY ([OwnerId], [ShipId]),
    CONSTRAINT [FK_ShipOwners_Owners_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Owners] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShipOwners_Ships_ShipId] FOREIGN KEY ([ShipId]) REFERENCES [Ships] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Owners]'))
    SET IDENTITY_INSERT [Owners] ON;
INSERT INTO [Owners] ([Id], [Name])
VALUES (1, N'Example Cruises'),
(2, N'Maritime Inc.');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Owners]'))
    SET IDENTITY_INSERT [Owners] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ImoNumber', N'Name', N'Tonnage', N'Type') AND [object_id] = OBJECT_ID(N'[Ships]'))
    SET IDENTITY_INSERT [Ships] ON;
INSERT INTO [Ships] ([Id], [ImoNumber], [Name], [Tonnage], [Type])
VALUES (1, N'1234567', N'Ocean Explorer', 5000.0, N'Cruise');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ImoNumber', N'Name', N'Tonnage', N'Type') AND [object_id] = OBJECT_ID(N'[Ships]'))
    SET IDENTITY_INSERT [Ships] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OwnerId', N'ShipId') AND [object_id] = OBJECT_ID(N'[ShipOwners]'))
    SET IDENTITY_INSERT [ShipOwners] ON;
INSERT INTO [ShipOwners] ([OwnerId], [ShipId])
VALUES (1, 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OwnerId', N'ShipId') AND [object_id] = OBJECT_ID(N'[ShipOwners]'))
    SET IDENTITY_INSERT [ShipOwners] OFF;

CREATE INDEX [IX_ShipOwners_ShipId] ON [ShipOwners] ([ShipId]);

CREATE UNIQUE INDEX [IX_Ships_ImoNumber] ON [Ships] ([ImoNumber]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250505134805_InitialCreate', N'9.0.4');

COMMIT;
GO

