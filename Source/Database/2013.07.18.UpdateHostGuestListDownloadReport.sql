-- Author:	Swati Agrawal
-- Summary:	Updated Report logic to show one user's bookings for one seating only once

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.18.UpdateHostGuestListDownloadReport'

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
SET Query= N'SELECT CASE WHEN ea.SeatingId = 0 THEN e.Start ELSE es.Start END AS [EventTime],  u.FirstName + '' '' + u.LastName as [Name],      am.Email, numberofguests AS NumberOfGuests, CASE WHEN ea.MenuOptionId = 0 THEN '''' ELSE emo.Title END as MenuOptionChosen, ISNULL(t.BookingRequirements, '''') AS BookingRequirements FROM  (SELECT userid, eventid, seatingid, menuoptionid, SUM(numberofguests) AS numberofguests FROM eventattendee WHERE eventid=@EventId group by userid, eventid, seatingid, menuoptionid)ea INNER JOIN [Event] e on ea.EventId = e.Id   INNER JOIN [User] u on u.Id = ea.UserId  INNER JOIN [aspnet_Membership] am  on am.UserId = u.Id   inner JOIN [EventSeating] es ON es.Id = ea.SeatingId     inner JOIN [EventMenuOption] emo ON emo.Id = ea.MenuOptionId    LEFT JOIN (SELECT t.EventId, t.SeatingId, t.MenuOptionId, t.UserId, max(ISNULL(tb.BookingRequirements,'''')) AS BookingRequirements FROM [Ticket] t  INNER JOIN [TicketBasket] tb on tb.Id = t.BasketId WHERE EventId = @EventId group by t.EventId, t.SeatingId, t.MenuOptionId, t.UserId) t on t.EventId = ea.EventId AND t.SeatingId = ea.SeatingId AND t.MenuOptionId = ea.MenuOptionId AND t.UserId = ea.UserId   ORDER BY 5, 3'
WHERE Name= 'Host Guest List Download' AND [Parameters]='@EventId=1' AND ReportType=1

	
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
