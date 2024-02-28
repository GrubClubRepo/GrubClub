-- Author:	Swati Agrawal
-- Summary:	This script creates SearchCategoryTag table 

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2014.12.03.CreateSearchCategoryTagTable'

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

CREATE TABLE [dbo].[SearchCategoryTag](
	[SearchCategoryId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
 CONSTRAINT [PK_SearchCategoryTag] PRIMARY KEY CLUSTERED 
(
	[SearchCategoryId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SearchCategoryTag]  WITH CHECK ADD  CONSTRAINT [FK_SearchCategoryTag_Tag] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tag] ([Id])
ALTER TABLE [dbo].[SearchCategoryTag] CHECK CONSTRAINT [FK_SearchCategoryTag_Tag]

ALTER TABLE [dbo].[SearchCategoryTag]  WITH CHECK ADD  CONSTRAINT [FK_SearchCategoryTag_SearchCategory] FOREIGN KEY([SearchCategoryId])
REFERENCES [dbo].[SearchCategory] ([Id])
ALTER TABLE [dbo].[SearchCategoryTag] CHECK CONSTRAINT [FK_SearchCategoryTag_SearchCategory]

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



