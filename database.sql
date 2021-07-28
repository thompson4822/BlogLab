create table ApplicationUser (
                                 ApplicationUserId INT NOT NULL IDENTITY (1,1),
                                 Username VARCHAR(20) NOT NULL,
                                 NormalizedUserName VARCHAR(20) NOT NULL,
                                 Email VARCHAR(30) NOT NULL,
                                 NormalizedEmail VARCHAR(30) NOT NULL ,
                                 Fullname VARCHAR(30) NULL ,
                                 PasswordHash NVARCHAR(MAX) NOT NULL ,
                                 PRIMARY KEY (ApplicationUserId)
)

create index [IX_ApplicationUser_NormalizedUsername] on [dbo].[ApplicationUser] ([NormalizedUsername])

create index [IX_ApplicationUser_NormalizedEmail] on [dbo].[ApplicationUser] ([NormalizedEmail])

CREATE TABLE Photo (
                       PhotoId INT NOT NULL IDENTITY (1,1),
                       ApplicationUserId INT NOT NULL ,
                       PublicId VARCHAR(50) NOT NULL ,
                       ImageUrl VARCHAR(250) NOT NULL ,
    [Description] VARCHAR(30) NOT NULL ,
    PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (PhotoId),
    FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId)
    )

CREATE TABLE Blog (
                      BlogId INT NOT NULL IDENTITY (1,1),
                      ApplicationUserId INT NOT NULL ,
                      PhotoId INT NULL ,
                      Title VARCHAR(50) NOT NULL ,
                      Content VARCHAR(MAX) NOT NULL ,
    PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ActiveInd BIT NOT NULL DEFAULT CONVERT(BIT,1),
    PRIMARY KEY (BlogId),
    FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId),
    FOREIGN KEY (PhotoId) REFERENCES Photo(PhotoId)
)

CREATE TABLE BlogComment (
                             BlogCommentId INT NOT NULL IDENTITY (1,1),
                             ParentBlogCommentId INT NULL ,
                             BlogId INT NOT NULL ,
                             ApplicationUserId INT NOT NULL ,
                             Content VARCHAR(300) NOT NULL ,
                             PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
                             UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
                             ActiveInd BIT NOT NULL DEFAULT CONVERT(BIT,1),
                             PRIMARY KEY (BlogCommentId),
                             FOREIGN KEY (BlogId) REFERENCES Blog(BlogId),
                             FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId)
)

CREATE SCHEMA [aggregate]

CREATE VIEW [aggregate].[Blog]
AS
SELECT
    t1.BlogId,
    t1.ApplicationUserId,
    t2.Username,
    t1.Title,
    t1.Content,
    t1.PhotoId,
    t1.PublishDate,
    t1.UpdateDate,
    t1.ActiveInd
FROM
    dbo.Blog t1
        INNER JOIN
    dbo.ApplicationUser t2 ON t1.ApplicationUserId = t2.ApplicationUserId

CREATE VIEW [aggregate].[BlogComment]
AS
SELECT
    t1.BlogCommentId,
    t1.ParentBlogCommentId,
    t1.BlogId,
    t1.Content,
    t2.Username,
    t1.ApplicationUserId,
    t1.PublishDate,
    t1.UpdateDate,
    t1.ActiveInd
FROM
    dbo.BlogComment t1
        INNER JOIN
    dbo.ApplicationUser t2 ON t1.ApplicationUserId = t2.ApplicationUserId

CREATE TYPE [dbo].[AccountType] AS TABLE
    (
    [Username] VARCHAR(20) NOT NULL ,
    [NormalizedUsername] VARCHAR(20) NOT NULL ,
    [Email] VARCHAR(30) NOT NULL ,
    [NormalizedEmail] VARCHAR(30) NOT NULL ,
    [Fullname] VARCHAR(30) NULL ,
    [PasswordHash] NVARCHAR(MAX) NOT NULL
    )

CREATE TYPE [dbo].[PhotoType] AS TABLE
    (
    [PublicId] VARCHAR(50) NOT NULL ,
    [ImageUrl] VARCHAR(250) NOT NULL ,
    [Description] VARCHAR(30) NOT NULL
    )

CREATE TYPE [dbo].[BlogType] AS TABLE
    (
    [BlogId] INT NOT NULL ,
    [Title] VARCHAR(50) NOT NULL ,
    [Content] VARCHAR(MAX) NOT NULL,
    [PhotoId] INT NULL
    )

CREATE TYPE [dbo].[BlogCommentType] AS TABLE
    (
    [BlogCommentId] INT NOT NULL ,
    [ParentBlogCommentId] INT NULL ,
    [BlogId] INT NOT NULL,
    [Content] VARCHAR(300) NOT NULL
    )

CREATE PROCEDURE [dbo].[Account_GetByUserName]
    @NormalizedUsername VARCHAR(20)
AS
SELECT
    ApplicationUserId,
    Username,
    NormalizedUserName,
    Email,
    NormalizedEmail,
    Fullname,
    PasswordHash
FROM
    ApplicationUser
WHERE
        NormalizedUserName = @NormalizedUsername

CREATE PROCEDURE [dbo].[Account_Insert]
    @Account AccountType READONLY 
AS
    INSERT INTO 
        ApplicationUser(
          Username, 
            NormalizedUserName, 
            Email, 
            NormalizedEmail,
            Fullname, 
            PasswordHash)
SELECT
    Username,
    NormalizedUserName,
    Email,
    NormalizedEmail,
    Fullname,
    PasswordHash
FROM
    @Account;

SELECT CAST(SCOPE_IDENTITY() AS INT)

CREATE PROCEDURE [dbo].[Blog_Delete]
    @BlogId INT
AS
UPDATE [dbo].BlogComment
SET ActiveInd = CONVERT(BIT, 0)
WHERE BlogId = @BlogId;

UPDATE [dbo].Blog
SET PhotoId = NULL,
    ActiveInd = CONVERT(BIT, 0)
WHERE
    BlogId = @BlogId;

CREATE PROCEDURE [dbo].Blog_Get
    @BlogId INT
AS
SELECT
    BlogId,
    ApplicationUserId,
    Username,
    Title,
    Content,
    PhotoId,
    PublishDate,
    UpdateDate
FROM
    [aggregate].[Blog]
WHERE BlogId = @BlogId AND ActiveInd = CONVERT(BIT, 1);

CREATE PROCEDURE [dbo].Blog_GetAll
    @Offset INT, @PageSize INT
AS
SELECT
    BlogId,
    ApplicationUserId,
    Username,
    Title,
    Content,
    PhotoId,
    PublishDate,
    UpdateDate
FROM
    [aggregate].[Blog] t1
WHERE
    t1.ActiveInd = CONVERT(BIT, 1)
ORDER BY
    t1.BlogId
OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY ;

SELECT COUNT(*) FROM [aggregate].Blog t1 WHERE t1.ActiveInd = CONVERT(BIT, 1);

CREATE PROCEDURE [dbo].Blog_GetAllFamous
AS
SELECT
    TOP 6
           t1.BlogId,
        t1.ApplicationUserId,
    t1.Username,
    t1.Title,
    t1.Content,
    t1.PhotoId,
    t1.PublishDate,
    t1.UpdateDate
FROM
    [aggregate].Blog t1
    INNER JOIN BlogComment BC on t1.BlogId = BC.BlogId
WHERE t1.ActiveInd = CONVERT(BIT, 1) AND BC.ActiveInd = CONVERT(BIT, 1)
GROUP BY
    t1.BlogId,
    t1.ApplicationUserId,
    t1.Username,
    t1.Title,
    t1.Content,
    t1.PhotoId,
    t1.PublishDate,
    t1.UpdateDate
ORDER BY
    COUNT(BC.BlogCommentId)
        DESC;

CREATE PROCEDURE [dbo].Blog_GetByUserId
    @ApplicationUserId INT
AS
SELECT
    BlogId,
    ApplicationUserId,
    Username,
    Title,
    Content,
    PhotoId,
    PublishDate,
    UpdateDate
FROM
    [aggregate].Blog t1
WHERE
    t1.ApplicationUserId = @ApplicationUserId AND
    t1.ActiveInd = CONVERT(BIT, 1);

CREATE PROCEDURE [dbo].Blog_Upsert  
    @Blog BlogType READONLY,
    @ApplicationUserId INT
AS
    MERGE INTO BLOG TARGET
    USING (
        SELECT 
               BlogId,
               @ApplicationUserId ApplicationUserId,
               Title,
               Content,
               PhotoId
        FROM 
            @Blog
    ) AS SOURCE
    ON
    (
        TARGET.BlogId = SOURCE.BlogId AND TARGET.ApplicationUserId = SOURCE.ApplicationUserId   
    )
    WHEN MATCHED THEN
UPDATE SET
    TARGET.Title = SOURCE.Title,
    TARGET.Content = SOURCE.Content,
    TARGET.PhotoId = SOURCE.PhotoId,
    TARGET.UpdateDate = GETDATE()
    WHEN NOT MATCHED BY TARGET THEN
INSERT (
ApplicationUserId, PhotoId, Title, Content
) VALUES (SOURCE.ApplicationUserId, SOURCE.PhotoId, SOURCE.Title, SOURCE.Content);

SELECT CAST(SCOPE_IDENTITY() AS INT );

CREATE PROCEDURE [dbo].BlogComment_Delete
    @BlogCommentId INT
AS
DROP TABLE IF EXISTS #BlogCommentsToBeDeleted;

WITH cte_blogComments AS (
    SELECT
        t1.BlogCommentId, t1.ParentBlogCommentId
    FROM
    [dbo].BlogComment t1
WHERE
    t1.BlogCommentId = @BlogCommentId
UNION ALL
SELECT
    t2.BlogCommentId, t2.ParentBlogCommentId
FROM
    [dbo].BlogComment t2
    INNER JOIN cte_blogComments t3
ON t3.BlogCommentId = t2.ParentBlogCommentId
    )
SELECT
    BlogCommentId,
    ParentBlogCommentId
INTO
    #BlogCommentsToBeDeleted
FROM
    cte_blogComments;

UPDATE t1
SET
    t1.ActiveInd = CONVERT(BIT, 0),
    t1.UpdateDate = GETDATE()
    FROM
        [dbo].BlogComment t1
    INNER JOIN #BlogCommentsToBeDeleted t2
ON t1.BlogCommentId = t2.BlogCommentId

CREATE PROCEDURE [dbo].BlogComment_Get
    @BlogCommentId INT
AS
SELECT
    BlogCommentId,
    ParentBlogCommentId,
    BlogId,
    Content,
    Username,
    ApplicationUserId,
    PublishDate,
    UpdateDate
FROM
    [aggregate].BlogComment
WHERE
    BlogCommentId = @BlogCommentId AND ActiveInd = CONVERT(BIT, 1);


CREATE PROCEDURE [dbo].BlogComment_GetAll
    @BlogId INT
AS
SELECT
    BlogCommentId,
    ParentBlogCommentId,
    BlogId,
    Content,
    Username,
    ApplicationUserId,
    PublishDate,
    UpdateDate
FROM [aggregate].BlogComment
WHERE BlogId = @BlogId AND ActiveInd = CONVERT(BIT, 1)
ORDER BY UpdateDate
        DESC

CREATE PROCEDURE [dbo].BlogComment_Upsert
    @BlogComment BlogCommentType READONLY ,
    @ApplicationUserId INT
AS
    MERGE INTO [dbo].BlogComment TARGET
    USING (
        SELECT 
               BC.BlogCommentId, 
               BC.ParentBlogCommentId,
               BC.BlogId,
               BC.Content,
               @ApplicationUserId ApplicationUserId
        FROM
            @BlogComment BC
    ) AS SOURCE
    ON (
       TARGET.BlogCommentId = SOURCE.BlogCommentId AND TARGET.ApplicationUserId = SOURCE.ApplicationUserId
    )
    WHEN MATCHED THEN
UPDATE SET
    TARGET.Content = SOURCE.Content,
    TARGET.UpdateDate = GETDATE()
    WHEN NOT MATCHED BY TARGET THEN
INSERT (
ParentBlogCommentId, BlogId, ApplicationUserId, Content
) VALUES (SOURCE.ParentBlogCommentId, SOURCE.BlogId, SOURCE.ApplicationUserId, SOURCE.Content);
SELECT CAST (SCOPE_IDENTITY() AS INT)


CREATE PROCEDURE [dbo].Photo_Delete
    @PhotoId INT
AS
DELETE FROM [dbo].Photo WHERE PhotoId = @PhotoId

CREATE PROCEDURE [dbo].Photo_Get
    @PhotoId INT
AS
SELECT
    PhotoId,
    ApplicationUserId,
    PublicId,
    ImageUrl,
    Description,
    PublishDate,
    UpdateDate
FROM [dbo].Photo
WHERE PhotoId = @PhotoId

CREATE PROCEDURE [dbo].Photo_GetByUserId
    @ApplicationUserId INT
AS
SELECT
    PhotoId,
    ApplicationUserId,
    PublicId,
    ImageUrl,
    Description,
    PublishDate,
    UpdateDate
FROM [dbo].Photo
WHERE ApplicationUserId = @ApplicationUserId

CREATE PROCEDURE [dbo].Photo_Insert
    @Photo PhotoType READONLY ,
    @ApplicationUserId INT
AS
    INSERT INTO [dbo].Photo (
         ApplicationUserId, 
         PublicId, 
         ImageUrl, 
         Description)
SELECT
    @ApplicationUserId,
    PublicId,
    ImageUrl,
    Description
FROM @Photo;

SELECT CAST(SCOPE_IDENTITY() AS INT)