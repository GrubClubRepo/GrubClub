-- Author:	Swati Agrawal
-- Summary:	This returns guest list for an event for a host

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.02.HostGuestListDownload'

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
           ('Host Guest List Download'
           ,'@EventId=1'
           ,1
           ,N'SELECT DISTINCT CASE WHEN ea.SeatingId = 0 THEN e.Start ELSE es.Start END AS [EventTime],  u.FirstName + '' '' + u.LastName as [Name],  
  am.Email, ea.NumberOfGuests as NumberOfTickets, CASE WHEN ea.MenuOptionId = 0 THEN '''' ELSE emo.Title END as MenuOptionChosen, ISNULL(tb.BookingRequirements,'''') AS BookingRequirements
   FROM [Event] e 
   INNER JOIN EventAttendee ea on ea.EventId = e.Id  
   INNER JOIN [User] u on u.Id = ea.UserId   
   INNER JOIN [aspnet_Membership] am  on am.UserId = u.Id  
   LEFT JOIN [Ticket] t on t.EventId = ea.EventId AND t.SeatingId = ea.SeatingId AND t.MenuOptionId = ea.MenuOptionId AND t.UserId = ea.UserId
   INNER JOIN [TicketBasket] tb on tb.Id = t.BasketId
   LEFT JOIN [EventSeating] es ON es.Id = ea.SeatingId
   LEFT JOIN [EventMenuOption] emo ON emo.Id = ea.MenuOptionId
   WHERE e.Id = @EventId  
   ORDER BY 1')

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


