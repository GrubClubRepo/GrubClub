-- Author:	Josh Helmink
-- Summary:	This add reserved seating to search as well as additional keyword search

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2012.12.27.EventReports'

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
	
INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('Events List'
           ,'@StartDate=2013-01-01;@EndDate=2013-12-31'
           ,'
SELECT * 
FROM [Event]
WHERE [Event].Start BETWEEN @StartDate AND @EndDate
ORDER BY [Event].Start ASC
           '
           ,1)
           
           
INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('Event Details'
           ,'@EventId=1'
           ,'
SELECT * 
FROM [Event]
INNER JOIN SupperClub on SupperClub.Id = [Event].SupperClubId
WHERE [Event].Id = @EventId
           '
           ,1)


INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('Event Guest List'
           ,'@EventId=1'
           ,'
SELECT 
[Event].Id as EventId,
[Event].Name as EventName,
[Event].Start,
[User].FirstName + '' '' + [User].LastName as [Name],
[aspnet_Membership].Email,
[EventAttendee].NumberOfGuests as NumberOfTickets
FROM [Event]
INNER JOIN EventAttendee on EventAttendee.EventId = [Event].Id
INNER JOIN [User] on [User].Id = EventAttendee.UserId 
INNER JOIN [aspnet_Membership] on [aspnet_Membership].UserId = [User].Id
WHERE [Event].Id = @EventId
           '
           ,1)

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