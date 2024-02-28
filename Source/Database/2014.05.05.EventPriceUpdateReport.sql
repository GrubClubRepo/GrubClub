-- Author:	Swati Agrawal
-- Summary:	This adds new reports in Event Section for checking the event price update log

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.05.EventPriceUpdateReport'

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
	
INSERT INTO [dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[ReportType]
           ,[Query]
           )
     VALUES
           ('Event Price Update Log Report'
           ,'@EventId=1'
           ,1
           ,N'SELECT e.name AS EventName, (CASE WHEN emo.Id = 0 THEN '''' ELSE emo.Title END) AS MenuOption, u.firstname + '' '' + u.lastname AS UserName, asp.Email, epcl.[Date] AS UpdateDateTime, (epcl.OldPrice * 1.1) AS OldPrice, (epcl.NewPrice * 1.1) AS NewPrice
from EventPriceChangeLog epcl 
INNER JOIN [Event] e on epcl.EventId = e.Id
INNER JOIN [User] u on u.Id = epcl.UserId
INNER JOIN aspnet_Membership asp on asp.UserId=u.Id
LEFT JOIN EventMenuOption emo on emo.Id = epcl.MenuOptionId 
WHERE epcl.EventId = @EventId')

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


