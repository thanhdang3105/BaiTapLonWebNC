USE WebNC
GO

CREATE PROCEDURE SelectAuthWithID @ID int
AS
IF @ID > 0
    SELECT TOP 1 * 
    FROM Auth 
    WHERE ID = @ID 
GO

CREATE PROCEDURE SelectAuthWithEmailAndPWD @Username nvarchar(max), @Password nvarchar(max)
AS
IF NOT (@Username = '' OR @Password = '')
    SELECT TOP 1 * 
    FROM Auth 
    WHERE username = @Username AND password = @Password 
    ORDER BY ID DESC
GO

CREATE PROCEDURE SelectAuthWithEmail @Username nvarchar(max)
AS
IF NOT (@Username = '')
    SELECT TOP 1 * FROM Auth WHERE username = @Username ORDER BY ID DESC;
GO

CREATE PROCEDURE SelectUserWithID @ID int
AS
IF @ID > 0
    SELECT TOP 1 * FROM Users WHERE ID = @ID  ORDER BY ID DESC;
GO

CREATE PROCEDURE InsertUserInfo @Name nvarchar(max), @Email NVARCHAR(max), @Birthday NVARCHAR(12), @Phone NVARCHAR(12), @Sex BIT
AS
IF @Sex > 1 OR @sex < 0
    Set @Sex = 1
INSERT INTO Users ([name],[email],[birthday],[phone],[sex])
OUTPUT Inserted.ID
VALUES (N''+@Name, N''+@Email, N''+@Birthday, N''+@Phone, @Sex);
GO

CREATE PROCEDURE UpdateUserInfo @ID int, @Name NVARCHAR(max), @Birthday NVARCHAR(15), @Phone NVARCHAR(12), @Sex BIT
AS
IF NOT (@ID < 0)
    UPDATE Users SET [name] = @Name, [birthday] = @Birthday, [phone] = @Phone, [sex] = @Sex OUTPUT inserted.ID WHERE ID = @ID;
GO

CREATE PROCEDURE InsertAuthInfo @Username nvarchar(max), @Password NVARCHAR(max), @userID INT
AS
IF NOT (@Username = '' OR @Password = '' OR @userID < 1)
    INSERT INTO Auth ([username],[password],[userID])
    OUTPUT Inserted.ID
    VALUES (N''+@Username, N''+@Password, N''+@userID);
GO

CREATE PROCEDURE UpdateAuthLocked @ID int, @IsLocked BIT
AS
IF NOT (@ID = '')
    UPDATE Auth SET [locked] = @IsLocked , [updatedAt] = (GETDATE()) OUTPUT inserted.ID WHERE ID = @ID;
GO

CREATE PROCEDURE UpdateAuthPassword @ID int, @Password NVARCHAR(max)
AS
IF NOT (@Password = '' OR @ID = '')
    UPDATE Auth SET [password] = @Password , [updatedAt] = (GETDATE()) OUTPUT inserted.ID WHERE ID = @ID;
GO

CREATE PROCEDURE UpdateAuthRole @ID int, @Role NVARCHAR(10)
AS
IF NOT (@Role = '' OR @ID = '')
    UPDATE Auth SET [role] = @Role , [updatedAt] = (GETDATE()) OUTPUT inserted.ID WHERE ID = @ID;
GO

CREATE PROCEDURE InsertCategoryInfo @Name NVARCHAR(100), @Key NVARCHAR(100)
AS
    IF NOT (@Name = '' OR @Key = '')
        INSERT INTO Category ([name],[key]) 
        OUTPUT @Key
        VALUES (N''+@Name, N''+@Key)
GO

CREATE PROCEDURE [dbo].[SelectCategoryWithFilter] @Filter NVARCHAR(MAX), @Sort NVARCHAR(MAX)
AS
DECLARE @sql NVARCHAR(MAX)
IF @Sort = ''
    SET @Sort = 'name DESC'
IF NOT @Filter = ''
    SET @sql = N'SELECT * FROM Category WHERE ' + @Filter + ' ORDER BY ' + @Sort
ELSE
    SET @sql = N'SELECT * FROM Category ORDER BY ' + @Sort
EXECUTE sp_executesql @sql
GO

CREATE PROCEDURE [dbo].[InsertBookInfo] @Name NVARCHAR(MAX), @Author NVARCHAR(MAX) = '', @Category NVARCHAR(100), @Content NVARCHAR(Max), @ImgSrc NVARCHAR(MAX)
AS
    IF NOT (@Name = '' OR @Category = '')
        IF @Author = ''
            INSERT INTO Books ([name],[category],[content],[imgSrc]) 
            OUTPUT Inserted.ID
            VALUES (N''+@Name, N''+@Category, N''+@Content, N''+@ImgSrc)
        ELSE
            INSERT INTO Books ([name],[author],[category],[content],[imgSrc]) 
            OUTPUT Inserted.ID
            VALUES (N''+@Name, N'' +@Author, N''+@Category, N''+@Content, N''+@ImgSrc)
GO

CREATE PROCEDURE [dbo].[UpdateBookInfo] @ID int, @Name NVARCHAR(MAX), @Author NVARCHAR(MAX) = '', @Category NVARCHAR(100), @Content NVARCHAR(Max), @ImgSrc NVARCHAR(MAX)
AS
IF NOT (@Name = '' OR @Category = '' OR @ID < 1)
    IF @Author = ''
        UPDATE [dbo].[Books] SET [name] = N''+@Name, [author] = 'Unknown Author', [category] = N''+@Category, [content] = N''+@Content, [imgSrc] = N''+@ImgSrc , [updatedAt] = (GETDATE()) WHERE ID = @ID;
    ELSE
        UPDATE [dbo].[Books] SET [name] = N''+@Name, [author] = N'' +@Author, [category] = N''+@Category, [content] = N''+@Content, [imgSrc] = N''+@ImgSrc , [updatedAt] = (GETDATE()) WHERE ID = @ID;
GO

CREATE PROCEDURE DeleteRowWithID @ID int, @TableName NVARCHAR(MAX)
AS
IF NOT (@TableName = '' OR @ID < 1)
    DECLARE @sql NVARCHAR(MAX)
    SET @sql = N'DELETE FROM ' + @TableName + ' WHERE [ID] = ' + CONVERT(NVARCHAR(12), @ID)
    EXECUTE sp_executesql @sql
GO

CREATE PROCEDURE SelectBooksWithID @ID int
AS
IF NOT @ID < 1
    SELECT TOP 1 * FROM Books WHERE [ID] = @ID ORDER BY ID DESC;
GO

CREATE PROCEDURE [dbo].[SelectBooksWithFilter] @ID int = 0, @Name NVARCHAR(MAX) = '', @Author NVARCHAR(MAX) = '', @Category NVARCHAR(100) = '', @Sort NVARCHAR(MAX), @Limit int, @Skip int
AS
IF @Skip is null OR @Skip < 0
    SET @Skip = 0
ELSE
    SET @Skip = @Skip * @Limit
IF @Limit = null
    SET @Limit = 10
IF @ID > 0
    SELECT Books.ID, Books.name, author, Category.name as categoryName, Category.[key] as categoryKey, content, imgSrc, [like], [view], createdAt, updatedAt 
    FROM Books, Category 
    WHERE Books.category = Category.[key] AND Books.ID = @ID 
    ORDER BY Books.ID DESC
ELSE
    IF @Category = '' OR @Category IS NULL
    SELECT Books.ID, Books.name, author, Category.name as categoryName, Category.[key] as categoryKey, content, imgSrc, [like], [view], createdAt, updatedAt 
        FROM Books, Category 
        WHERE Books.category = Category.[key] AND [Books].[name] LIKE N'%'+ @Name + '%' AND Books.author LIKE N'%' + @Author + '%'
        ORDER BY 
            CASE WHEN @Sort = 'ID DESC' THEN Books.ID END DESC,
                CASE WHEN @Sort = 'ID ASC' THEN Books.ID END ASC,
                CASE WHEN @Sort = 'like DESC' THEN Books.[like] END DESC,
                CASE WHEN @Sort = 'like ASC' THEN Books.[like] END ASC,
                CASE WHEN @Sort = 'view DESC' THEN Books.[view] END DESC,
                CASE WHEN @Sort = 'view ASC' THEN Books.[view] END ASC,
                CASE WHEN @Sort = 'createdAt DESC' THEN Books.[createdAt] END DESC,
                CASE WHEN @Sort = 'createdAt ASC' THEN Books.[createdAt] END ASC,
                CASE WHEN @Sort = 'updatedAt ASC' THEN Books.[updatedAt] END ASC,
                CASE WHEN @Sort = 'updatedAt DESC' THEN Books.[updatedAt] ELSE Books.ID END DESC
        OFFSET @Skip ROWS 
        FETCH NEXT @Limit ROWS ONLY
    ELSE 
    SELECT Books.ID, Books.name, author, Category.name as categoryName, Category.[key] as categoryKey, content, imgSrc, [like], [view], createdAt, updatedAt 
            FROM Books, Category 
            WHERE Books.category = Category.[key] AND [Books].[category] = N''+ @Category AND ([Books].[name] LIKE N'%'+ @Name + '%' AND Books.author LIKE N'%' + @Author + '%')
            ORDER BY 
                CASE WHEN @Sort = 'ID DESC' THEN Books.ID END DESC,
                CASE WHEN @Sort = 'ID ASC' THEN Books.ID END ASC,
                CASE WHEN @Sort = 'like DESC' THEN Books.[like] END DESC,
                CASE WHEN @Sort = 'like ASC' THEN Books.[like] END ASC,
                CASE WHEN @Sort = 'view DESC' THEN Books.[view] END DESC,
                CASE WHEN @Sort = 'view ASC' THEN Books.[view] END ASC,
                CASE WHEN @Sort = 'createdAt DESC' THEN Books.[createdAt] END DESC,
                CASE WHEN @Sort = 'createdAt ASC' THEN Books.[createdAt] END ASC,
                CASE WHEN @Sort = 'updatedAt ASC' THEN Books.[updatedAt] END ASC,
                CASE WHEN @Sort = 'updatedAt DESC' THEN Books.[updatedAt] ELSE Books.ID END DESC
            OFFSET @Skip ROWS 
            FETCH NEXT @Limit ROWS ONLY
GO

CREATE PROCEDURE [dbo].[SelectBooksWithSearch] @Search NVARCHAR(MAX), @Sort NVARCHAR(MAX), @Limit int, @Skip int
AS
IF @Skip is null OR @Skip < 0
    SET @Skip = 0
ELSE
    SET @Skip = @Skip * @Limit
IF @Limit = null
    SET @Limit = 10
BEGIN
    SELECT Books.ID, Books.name, author, Category.name as categoryName, Category.[key] as categoryKey, content, imgSrc, [like], [view], createdAt, updatedAt 
            FROM Books, Category 
            WHERE Books.category = Category.[key] 
            AND ([Books].[name] LIKE N'%'+ @Search + '%' OR Books.author LIKE N'%' + @Search + '%' OR [Books].[category] LIKE N'%'+ @Search + '%' OR Category.[name] LIKE N'%'+ @Search + '%')
            ORDER BY 
                CASE WHEN @Sort = 'ID DESC' THEN Books.ID END DESC,
                CASE WHEN @Sort = 'ID ASC' THEN Books.ID END ASC,
                CASE WHEN @Sort = 'like DESC' THEN Books.[like] END DESC,
                CASE WHEN @Sort = 'like ASC' THEN Books.[like] END ASC,
                CASE WHEN @Sort = 'view DESC' THEN Books.[view] END DESC,
                CASE WHEN @Sort = 'view ASC' THEN Books.[view] END ASC,
                CASE WHEN @Sort = 'createdAt DESC' THEN Books.[createdAt] END DESC,
                CASE WHEN @Sort = 'createdAt ASC' THEN Books.[createdAt] END ASC,
                CASE WHEN @Sort = 'updatedAt ASC' THEN Books.[updatedAt] END ASC,
                CASE WHEN @Sort = 'updatedAt DESC' THEN Books.[updatedAt] ELSE Books.ID END DESC
            OFFSET @Skip ROWS 
            FETCH NEXT @Limit ROWS ONLY
END
GO

CREATE PROCEDURE CountBooksWithFilter @Filter NVARCHAR(MAX)
AS
IF @Filter = ''
    SELECT COUNT(ID) AS count FROM Books
ELSE 
    DECLARE @sql NVARCHAR(MAX)
    SET @sql = N'SELECT COUNT(ID) AS count FROM Books WHERE ' + @Filter
    EXECUTE sp_executesql @sql
GO

CREATE PROCEDURE SelectAuthAndUser @Filter NVARCHAR(MAX), @Sort NVARCHAR(MAX), @Skip int, @Limit int
AS
DECLARE @sql NVARCHAR(max) 
SET @sql = N'SELECT [Auth].[ID] AS ID, [Users].[name], [Auth].[username] AS email, birthday, phone, sex, [role], [locked]  from Auth, Users where userID = [Users].[ID]'
IF @Sort IS NULL OR @Sort = ''
    SET @Sort = 'ID DESC'
IF @Filter IS NULL OR @Filter = ''
    SET @sql = @sql + ' ORDER BY ' + @Sort + ' OFFSET ' + CONVERT(NVARCHAR(12), @Skip) + ' ROWS FETCH NEXT ' + CONVERT(NVARCHAR(12), @Limit) + ' ROWS ONLY'
ELSE 
    SET @sql = @sql + ' AND (' + @Filter + ') ORDER BY ' + @Sort + ' OFFSET ' + CONVERT(NVARCHAR(12), @Skip) + ' ROWS FETCH NEXT ' + CONVERT(NVARCHAR(12), @Limit) + ' ROWS ONLY'
EXECUTE sp_executesql @sql
GO

CREATE PROCEDURE CountAuth @Filter NVARCHAR(MAX)
AS
IF @Filter = ''
    SELECT COUNT(ID) AS count FROM Auth
ELSE 
    DECLARE @sql NVARCHAR(MAX)
    SET @sql = N'SELECT COUNT(ID) AS count FROM Auth WHERE ' + @Filter
    EXECUTE sp_executesql @sql
GO

CREATE PROCEDURE SelectHistoryActionWithBook @ID INT, @UserID INT, @BookID INT, @Action NVARCHAR(50), @Sort NVARCHAR(MAX), @Skip INT, @Limit INT
AS
IF @ID > 0 
    SELECT HistoryUserBook.*, Books.name, Books.author, Category.name as categoryName, Category.[key] as categoryKey, Books.content, Books.imgSrc, Books.[like], Books.[view]
    FROM HistoryUserBook, Books, Category
    WHERE HistoryUserBook.ID = @ID AND Books.ID = HistoryUserBook.bookID AND Books.category = Category.[key]
ELSE 
    IF @Action IS NULL OR @Action = ''
        SELECT HistoryUserBook.*, Books.name, Books.author, Category.name as categoryName, Category.[key] as categoryKey, Books.content, Books.imgSrc, Books.[like], Books.[view]
        FROM HistoryUserBook, Books, Category
        WHERE Books.ID = HistoryUserBook.bookID AND Books.category = Category.[key] AND HistoryUserBook.bookID = @BookID AND HistoryUserBook.userID = @UserID
        ORDER BY 
            CASE WHEN @Sort = 'ID DESC' THEN HistoryUserBook.ID END DESC,
            CASE WHEN @Sort = 'ID ASC' THEN HistoryUserBook.ID END ASC,
            CASE WHEN @Sort = 'createdAt DESC' THEN HistoryUserBook.[createdAt] END DESC,
            CASE WHEN @Sort = 'createdAt ASC' THEN HistoryUserBook.[createdAt] END ASC,
            CASE WHEN @Sort = 'updatedAt ASC' THEN HistoryUserBook.[updatedAt] END ASC,
            CASE WHEN @Sort = 'updatedAt DESC' THEN HistoryUserBook.[updatedAt] ELSE HistoryUserBook.ID END DESC
        OFFSET @Skip ROWS 
        FETCH NEXT @Limit ROWS ONLY
    ELSE 
        IF NOT (@UserID IS NULL OR @UserID = '') AND NOT (@BookID IS NULL OR @BookID = '') 
            SELECT HistoryUserBook.*, Books.name, Books.author, Category.name as categoryName, Category.[key] as categoryKey, Books.content, Books.imgSrc, Books.[like], Books.[view]
            FROM HistoryUserBook, Books, Category
            WHERE Books.ID = HistoryUserBook.bookID AND Books.category = Category.[key] AND HistoryUserBook.bookID = @BookID AND HistoryUserBook.userID = @UserID AND HistoryUserBook.action = @Action
            ORDER BY 
                CASE WHEN @Sort = 'ID DESC' THEN HistoryUserBook.ID END DESC,
                CASE WHEN @Sort = 'ID ASC' THEN HistoryUserBook.ID END ASC,
                CASE WHEN @Sort = 'createdAt DESC' THEN HistoryUserBook.[createdAt] END DESC,
                CASE WHEN @Sort = 'createdAt ASC' THEN HistoryUserBook.[createdAt] END ASC,
                CASE WHEN @Sort = 'updatedAt ASC' THEN HistoryUserBook.[updatedAt] END ASC,
                CASE WHEN @Sort = 'updatedAt DESC' THEN HistoryUserBook.[updatedAt] ELSE HistoryUserBook.ID END DESC
            OFFSET @Skip ROWS 
            FETCH NEXT @Limit ROWS ONLY
        ELSE 
            SELECT HistoryUserBook.*, Books.name, Books.author, Category.name as categoryName, Category.[key] as categoryKey, Books.content, Books.imgSrc, Books.[like], Books.[view]
            FROM HistoryUserBook, Books, Category
            WHERE Books.ID = HistoryUserBook.bookID AND Books.category = Category.[key] AND
            ((HistoryUserBook.userID = @UserID AND HistoryUserBook.action = @Action)
            OR
            (HistoryUserBook.bookID = @BookID AND HistoryUserBook.action = @Action))
            ORDER BY 
                CASE WHEN @Sort = 'ID DESC' THEN HistoryUserBook.ID END DESC,
                CASE WHEN @Sort = 'ID ASC' THEN HistoryUserBook.ID END ASC,
                CASE WHEN @Sort = 'createdAt DESC' THEN HistoryUserBook.[createdAt] END DESC,
                CASE WHEN @Sort = 'createdAt ASC' THEN HistoryUserBook.[createdAt] END ASC,
                CASE WHEN @Sort = 'updatedAt ASC' THEN HistoryUserBook.[updatedAt] END ASC,
                CASE WHEN @Sort = 'updatedAt DESC' THEN HistoryUserBook.[updatedAt] ELSE HistoryUserBook.ID END DESC
            OFFSET @Skip ROWS 
            FETCH NEXT @Limit ROWS ONLY
GO

CREATE PROCEDURE InsertHistoryActionWithBook @UserID INT, @BookID INT, @Action NVARCHAR(50)
AS
IF NOT (@UserID IS NULL OR @BookID IS NULL OR @UserID = '' OR @BookID = '')
    IF @Action IS NULL OR @Action = ''
        INSERT INTO HistoryUserBook (userID, bookID) OUTPUT Inserted.ID VALUES(@userID, @BookID)
    ELSE 
        INSERT INTO HistoryUserBook (userID, bookID,action) OUTPUT Inserted.ID VALUES(@userID, @BookID, @Action)
GO

CREATE PROCEDURE UpdateHistoryActionWithBook @ID INT, @Action NVARCHAR(50)
AS
    UPDATE HistoryUserBook SET [action] = @Action , [updatedAt] = (GETUTCDATE()) OUTPUT inserted.ID WHERE ID = @ID;
GO