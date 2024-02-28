-- Author:	Swati Agrawal
-- Summary:	This adds new columns to Review table

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.20.AddColumnsToReviewTable'

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

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Review'
AND COLUMN_NAME = 'FoodRating')
ALTER TABLE [DBO].[Review] ADD FoodRating decimal(18,4) NULL

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Review'
AND COLUMN_NAME = 'AmbienceRating')
ALTER TABLE [DBO].[Review] ADD AmbienceRating decimal(18,4) NULL

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Review'
AND COLUMN_NAME = 'Rating')
 ALTER TABLE dbo.Review ALTER COLUMN Rating decimal(18,4)
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


