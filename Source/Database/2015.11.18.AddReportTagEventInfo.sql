-- Author:	Swati Agrawal
-- Summary:	This returns Tag and it's related events

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.18.AddReportTagEventInfo'

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
           ('Tag Event Info Report'
           ,NULL
           ,5
           ,N'Select T.Name, count(EA.EventID) AS NumberOfEvents, SUM(Revenue) AS NetRevenue, dbo.[CommaSeperatedProfiles](ET.TagId) AS ProfileNames
FROM dbo.EventTag ET WITH(NOLOCK)
INNER JOIN dbo.Tag T WITH(NOLOCK) ON ET.TagId = T.Id 
INNER JOIN dbo.Event E WITH(NOLOCK) ON ET.EventId = E.Id 
INNER JOIN dbo.SupperClub S WITH(NOLOCK) ON S.Id = E.SupperClubId
INNER JOIN (Select EventId, SUM(TotalPrice) AS Revenue from dbo.EventAttendee WITH(NOLOCK) group by EventId) EA ON EA.EventId = E.Id
GROUP BY T.Name, ET.TagId
')
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




