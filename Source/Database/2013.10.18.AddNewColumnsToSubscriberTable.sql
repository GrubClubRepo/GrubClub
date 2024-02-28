-- Author:	Swati Agrawal
-- Summary:	This adds the FirstName, LastName, UserId, SubscriberType column to Subscriber table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.10.18.AddNewColumnsToSubscriberTable'

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


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Subscriber'
AND COLUMN_NAME = 'FirstName')
ALTER TABLE [dbo].[Subscriber] ADD FirstName NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Subscriber'
AND COLUMN_NAME = 'LastName')
ALTER TABLE [dbo].[Subscriber] ADD LastName NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Subscriber'
AND COLUMN_NAME = 'SubscriberType')
ALTER TABLE [dbo].[Subscriber] ADD SubscriberType [int] NULL 
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Subscriber'
AND COLUMN_NAME = 'UserId')
ALTER TABLE [dbo].[Subscriber] ADD UserId [uniqueidentifier] NULL

EXEC sp_executesql N'UPDATE [dbo].[Subscriber] SET [SubscriberType]=1 WHERE [SubscriberType] IS NULL
ALTER TABLE [dbo].[Subscriber] ALTER COLUMN SubscriberType [int] NOT NULL'

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

