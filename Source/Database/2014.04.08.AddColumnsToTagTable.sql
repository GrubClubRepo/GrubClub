-- Author:	Swati Agrawal
-- Summary:	This adds new columns to Tag table

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.04.08.AddColumnsToTagTable'

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

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tag'
AND COLUMN_NAME = 'MetaTitle')
ALTER TABLE [DBO].[Tag] ADD MetaTitle nvarchar(100) NOT NULL DEFAULT('Pop Up Restaurants Near You| Eat Out with Grub Club')

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tag'
AND COLUMN_NAME = 'MetaDescription')
ALTER TABLE [DBO].[Tag] ADD MetaDescription nvarchar(256) NOT NULL DEFAULT('Search for the latest Pop Up Restaurants and Supperclubs in your area. Be the first to try our creative chef’s latest creations.')

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tag'
AND COLUMN_NAME = 'H2Tag')
ALTER TABLE [DBO].[Tag] ADD H2Tag nvarchar(150) NOT NULL DEFAULT('Check out these great Grub Clubs. Filter by Price, Cuisine and Category.')

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tag'
AND COLUMN_NAME = 'TagDescription')
ALTER TABLE [DBO].[Tag] ADD TagDescription nvarchar(1024) NOT NULL DEFAULT('')

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


