CREATE TABLE [dbo].[Users] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [name]      NVARCHAR (MAX) NOT NULL,
    [email]     NVARCHAR (MAX) NOT NULL,
    [birthday]  NVARCHAR (15)  NULL,
    [phone]     NVARCHAR (15)  NULL,
    [sex]       BIT            CONSTRAINT [DEFAULT_Users_sex] DEFAULT ((0)) NULL,
    [createdAt] DATETIME       CONSTRAINT [DEFAULT_Users_createdAt] DEFAULT (getutcdate()) NULL,
    [updatedAt] DATETIME       CONSTRAINT [DEFAULT_Users_column_1] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([ID] ASC)
);

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0=Female;1=Male', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'sex';

CREATE TABLE [dbo].[Auth] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [username]  NVARCHAR (MAX) NOT NULL,
    [password]  NVARCHAR (MAX) NOT NULL,
    [userID]    INT            NOT NULL,
    [role]      NVARCHAR (MAX) CONSTRAINT [DEFAULT_Auth_role] DEFAULT ('view') NULL,
    [locked]    BIT            CONSTRAINT [DEFAULT_Auth_locked] DEFAULT ((0)) NULL,
    [createdAt] DATETIME       CONSTRAINT [DEFAULT_Auth_createdAt] DEFAULT (getutcdate()) NULL,
    [updatedAt] DATETIME       CONSTRAINT [DEFAULT_Auth_updatedAt] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [FK_Auth_Users] FOREIGN KEY ([userID]) REFERENCES [dbo].[Users] ([ID])
);



CREATE TABLE [dbo].[Category] (
    [name] NVARCHAR (50) NOT NULL,
    [key]  NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([key] ASC)
);

CREATE TABLE [dbo].[Books] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [name]      NVARCHAR (MAX) NOT NULL,
    [author]    NVARCHAR (MAX) CONSTRAINT [DEFAULT_Books_author] DEFAULT ('Unknow Author') NULL,
    [category]  NVARCHAR (50)  NOT NULL,
    [content]   NVARCHAR (MAX) CONSTRAINT [DEFAULT_Books_content] DEFAULT ('Please wait content posted') NULL,
    [imgSrc]    NVARCHAR (MAX) NOT NULL,
    [like]      INT            CONSTRAINT [DEFAULT_Books_like] DEFAULT ((0)) NOT NULL,
    [view]      INT            CONSTRAINT [DEFAULT_Books_view] DEFAULT ((0)) NOT NULL,
    [createdAt] DATETIME       CONSTRAINT [DEFAULT_Books_createdAt] DEFAULT (getutcdate()) NULL,
    [updatedAt] DATETIME       CONSTRAINT [DEFAULT_Books_updatedAt] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Books_Category] FOREIGN KEY ([category]) REFERENCES [dbo].[Category] ([key])
);

CREATE TABLE [dbo].[HistoryUserBook] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [userID]    INT           NULL,
    [bookID]    INT           NOT NULL,
    [action]    NVARCHAR (50) CONSTRAINT [DEFAULT_HistoryUserBook_action] DEFAULT ('view') NOT NULL,
    [createdAt] DATETIME      CONSTRAINT [DEFAULT_HistoryUserBook_createdAt] DEFAULT (getutcdate()) NULL,
    [updatedAt] DATETIME      CONSTRAINT [DEFAULT_HistoryUserBook_updatedAt] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_HistoryUserBook] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_HistoryUserBook_Books] FOREIGN KEY ([bookID]) REFERENCES [dbo].[Books] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_HistoryUserBook_Users] FOREIGN KEY ([userID]) REFERENCES [dbo].[Users] ([ID]) ON DELETE SET NULL
);

