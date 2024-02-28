-- Author:	Swati Agrawal
-- Summary:	This returns all events during a defined time period

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2017.03.30.AddNewEventListReport'

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
           ('Sid''s Event List Report'
           ,'@StartDate=01-01-2017;@EndDate=31-12-2017'
           ,1
           ,N'select
				e.name as EventName,
				s.name as GrubClub,
				e.Id as EventId,
				a.FirstName + '' '' + a.LastName as HostedBy,
				e.start as EventDate,
				CONVERT(varchar(3), e.start, 100) + '' '' + CONVERT(varchar(4),DATEPART(yyyy, e.start)) as [Month]
				from dbo.Event e with(nolock)
				inner join dbo.SupperClub s with(nolock) on e.supperclubid = s.id
				inner join dbo.[user] a with(nolock) on a.id = s.userid
				where e.status=1 and s.active=1 and (e.start between @StartDate and @EndDate)
				order by e.start')
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





