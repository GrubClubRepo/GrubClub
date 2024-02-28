-- Author:	Supraja
-- Summary:	 Event Commission change log report

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.4.1.AddReportCommissionChangeLog'

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
           ('Event Commission Change Log Report'
           ,NULL
           ,1
           ,N'select l.EventId,e.Name,u.FirstName+'' ''+u.LastName as UserName,l.OldCommission,l.NewCommission,l.Date from eventcommissionchangelog l

join [user] u on l.userid=u.id
join [Event] e on l.eventid=e.id')
   
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




