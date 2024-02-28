-- Author:	Swati Agrawal
-- Summary:	This adds the Segmwnt User Table

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.04.07.AddSegmentUserTable'

PRINT '-------------------------------------------------------------------------------'
PRINT @ScriptCode
PRINT '-------------------------------------------------------------------------------'

IF(EXISTS(SELECT * FROM dbo.ScriptUpdate WHERE ScriptCode=@ScriptCode))
  PRINT N'Script was executed before'
ELSE
  BEGIN TRY
    BEGIN TRANSACTION

    set ANSI_NULLS ON
    set QUOTED_IDENTIFIER ON
    
----SCRIPT GOES BELOW THIS LINE--------------------------------------------------------
CREATE TABLE [dbo].[SegmentUser](
	[UserId] [uniqueidentifier] NOT NULL,
	[SegmentUserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SegmentUser] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SegmentUser]  WITH CHECK ADD  CONSTRAINT [FK_SegmentUser_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ALTER TABLE [dbo].[SegmentUser] CHECK CONSTRAINT [FK_SegmentUser_User]


----SCRIPT GOES ABOVE THIS LINE--------------------------------------------------------
    INSERT INTO dbo.ScriptUpdate(ScriptCode) VALUES(@ScriptCode)
  
    COMMIT
    PRINT N'Script executed'
  END TRY
  BEGIN CATCH
    ROLLBACK
    PRINT ERROR_MESSAGE()
    RAISERROR(N'Rolling back script.',11,1)
  END CATCH
PRINT '-------------------------------------------------------------------------------'
PRINT ''
