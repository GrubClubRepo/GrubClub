-- Author:	Swati
-- Summary:	added extra Columns in the report

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.12.01.UpdateBookingHistoryReportUpdated'

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
SET Query='select firstname, 
       lastname, 
       email, 
       EventName, 
       ProfileName, 
       EventDate, 
       BookingReferenceNumber, 
       BookingDate, 
       TicketsSold,                                     
       PricePaidByDiner,                   
       PriceAdvertised, 
       BYOB,
       TypeOfTransaction,
       CancellationDate AS CancelDate 
from 
((SELECT DISTINCT u.firstname, 
                u.lastname, 
                asm.email, 
                e.NAME              AS EventName, 
                s.NAME              AS ProfileName, 
                e.start             AS EventDate, 
                tb.bookingreference AS BookingReferenceNumber, 
                ea.bookingdate, 
                Sum(numberofguests) AS TicketsSold,                                     
                Cast(Round(Sum(ea.totalprice), 2) AS NUMERIC(36, 2)) AS PricePaidByDiner,                   
                Cast(Round((e.cost * 1.1 ), 2) AS NUMERIC(36, 2)) AS PriceAdvertised, 
                case when e.alcohol =1 then ''YES'' else ''NO'' end AS BYOB,
                ''Booking'' AS TypeOfTransaction,
                null as CancellationDate                   
                FROM   eventattendee ea WITH(nolock)                            
                INNER JOIN [user] u WITH(nolock)                  ON ea.userid = u.id                            
                INNER JOIN [aspnet_membership] asm WITH(nolock)   ON asm.userid = u.id                            
                INNER JOIN [event] e WITH(nolock)                  ON e.id = ea.eventid                            
                INNER JOIN [supperclub] s WITH(nolock)             ON s.id = e.supperclubid                            
                LEFT JOIN dbo.[ticketBasket] tb with(nolock) ON tb.id = ea.basketid and tb.userid = ea.userid                  
                WHERE  e.start BETWEEN @StartDate AND @EndDate                     
                GROUP  BY u.firstname,                                
                u.lastname,                                
                asm.email,                                
                e.NAME,                               
                s.NAME,                              
                e.start,                   
                e.cost,                   
                tb.bookingreference,                   
                ea.BookingDate,
                e.alcohol)
union all
(SELECT u.firstname, 
                u.lastname, 
                asm.email, 
                e.NAME              AS EventName, 
                s.NAME              AS ProfileName, 
                e.start             AS EventDate, 
                eat.bookingreference AS BookingReferenceNumber, 
                eat.bookingdateTime As BookingDate, 
                eat.numberofguests AS TicketsSold,                                     
                Cast(Round(eat.totalprice, 2) AS NUMERIC(36, 2)) AS PricePaidByDiner,                   
                Cast(Round((e.cost * 1.1 ), 2) AS NUMERIC(36, 2)) AS PriceAdvertised, 
                case when e.alcohol =1 then ''YES'' else ''NO'' end AS BYOB,
                ''Cancellation'' AS TypeOfTransaction,
                canceldatetime as CancellationDate 
                 FROM   bookingcancellationlog eat  
                INNER JOIN event e 
                        ON e.id = eat.eventid 
                INNER JOIN supperclub s 
                        ON s.id = e.supperclubid 
                INNER JOIN [user] u 
                        ON u.id = eat.userid 
                INNER JOIN aspnet_membership asm 
                        ON asm.userid = u.id 
               WHERE  e.start BETWEEN @StartDate AND @EndDate ) )temp'
WHERE Name= 'Booking History Report Updated' AND ReportType=3


	
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
