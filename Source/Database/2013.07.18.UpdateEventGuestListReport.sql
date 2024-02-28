-- Author:	Swati Agrawal
-- Summary:	Updated Report logic to show one user's bookings only once

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.18.UpdateEventGuestListReport'

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


UPDATE dbo.[Report]
SET Query= N'SELECT   [Event].Id as EventId,  [Event].Name as EventName,  [Event].Start,  [User].FirstName + '' '' + [User].LastName as [Name],  [aspnet_Membership].Email,  SUM([EventAttendee].NumberOfGuests) as NumberOfTickets  FROM [Event]  INNER JOIN EventAttendee on EventAttendee.EventId = [Event].Id  INNER JOIN [User] on [User].Id = EventAttendee.UserId   INNER JOIN [aspnet_Membership] on [aspnet_Membership].UserId = [User].Id  WHERE [Event].Id = @EventId  GROUP BY [Event].Id, [Event].Name, [Event].Start, [User].FirstName, [User].LastName, [aspnet_Membership].Email  '
WHERE Name= 'Event Guest List' AND [Parameters]='@EventId=1' AND ReportType=1

	
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
