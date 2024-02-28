-- Author:	Swati Agrawal
-- Summary:	This returns guests booking history

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.13.InsertGuestBookingHistory'

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
           ('Booking History Report'
           ,'@StartDate=01-01-2013;@EndDate=31-12-2014'
           ,3
           ,N'SELECT DISTINCT u.FirstName + '' '' + u.LastName AS GuestName, asm.Email, ISNULL(ea.BookingDate, tt.LastUpdated) AS DateBooked, e.Name AS EventName, s.name AS GrubClub, e.Start AS EventDate, tt.BookingReference AS BookingReferenceNumber, sum(NumberOfGuests), isnull(sum(ea.TotalPrice), sum(tt.TotalPrice)) AS PricePaid
FROM EventAttendee ea WITH(NOLOCK)
INNER JOIN [User] u WITH(NOLOCK) ON ea.UserId = u.Id
INNER JOIN [aspnet_membership] asm WITH(NOLOCK) ON asm.UserId = u.Id
INNER JOIN [event] e WITH(NOLOCK) ON e.Id=ea.EventId
INNER JOIN [supperclub] s WITH(NOLOCK) ON s.Id = e.SupperclubId
INNER JOIN 
(SELECT EventId, t.UserId, max(tb.BookingReference) AS BookingReference,max(tb.LastUpdated) AS LastUpdated, sum(totalprice) AS totalprice FROM
[ticket] t WITH(NOLOCK) 
INNER JOIN [ticketBasket] tb WITH(NOLOCK) ON tb.Id = t.BasketId WHERE tb.Status = ''Complete'' GROUP BY EventId, t.UserId) tt ON ea.eventId = tt.eventId and ea.userId = tt.userId
WHERE e.Start BETWEEN @StartDate AND @EndDate
GROUP BY u.FirstName,u.LastName,asm.Email,e.Name, s.name, e.Start, tt.BookingReference, ea.BookingDate, tt.LastUpdated')

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


