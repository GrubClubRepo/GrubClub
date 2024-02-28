-- Author:	Supraja
-- Summary:	Updated Report logic to show one user's bookings for one seating only once

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.24.UpdateHostGuestListDownloadReport'

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
SET Query= N'SELECT CASE 
         WHEN ea.seatingid = 0 THEN e.start 
         ELSE es.start 
       END                                                       AS [EventTime], 
       u.firstname + '' '' + u.lastname                            AS [Name], 
       am.email, 
       Isnull(u.contactnumber, '''')                               AS 
       ContactNumber, 
       numberofguests                                            AS 
       NumberOfGuests, 
       CASE 
         WHEN ea.menuoptionid = 0 THEN '''' 
         ELSE emo.title 
       END                                                       AS 
       MenuOptionChosen, 
       Replace(Isnull(t.bookingrequirements, ''''), ''&#x0D;'', '' '') AS 
       BookingRequirements ,
	   Sum(guestCharge) as GuestPrice
FROM   (SELECT et.userid, 
               eventid, 
               seatingid, 
               menuoptionid, 
			  
			   case when sv.voucherid  is null and  vc.id is null then sum(TotalPrice) 
			    when sv.voucherid is not null and vc.id is not null then sum(GuestBasePrice)-sum(Discount)
			   Else Sum(GuestBasePrice) end as guestCharge,
              Sum(numberofguests) AS numberofguests
			 
        FROM   eventattendee  et
		left join event en on et.eventid=en.id
		left join supperclub sc on sc.id=en.id
		
		left join voucher vc on vc.id = et.voucherid
		left join supperclubvoucher sv on sv.voucherid=vc.id
        WHERE  eventid = @EventId
        GROUP  BY et.userid, 
                  eventid, 
                  seatingid, 
                 menuoptionid
				   ,sc.id,
			   sv.voucherid,
			   et.id,vc.id)ea 
       INNER JOIN [event] e 
               ON ea.eventid = e.id 
       INNER JOIN [user] u 
               ON u.id = ea.userid 
       INNER JOIN [aspnet_membership] am 
               ON am.userid = u.id 
       INNER JOIN [eventseating] es 
               ON es.id = ea.seatingid 
       INNER JOIN [eventmenuoption] emo 
               ON emo.id = ea.menuoptionid 
       LEFT JOIN (SELECT DISTINCT t.eventid, 
                                  t.seatingid, 
                                  t.menuoptionid, 
                                  t.userid, 
                                  Isnull(el.br, '''') AS bookingrequirements
                  FROM   (SELECT eventid, 
                                 seatingid, 
                                 menuoptionid, 
                                 userid, 
                                 basketid 
                          FROM   ticket 
                          WHERE  eventid = @EventId)t 
                         CROSS apply (
										SELECT bookingrequirements + '',  '' AS [text()] 
										from(SELECT DISTINCT t.eventid, 
									  t.seatingid, 
									  t.menuoptionid, 
									  t.userid, 
									  Isnull(bookingrequirements, '''') AS bookingrequirements
									  FROM   ticket t
											  join ticketbasket tb 
										  on  tb.userid = t.userid  and     
										  tb.id=t.basketid                                       
												 where eventid=@EventId )tb WHERE  tb.userid = t.userid 
                                         AND tb.bookingrequirements IS NOT 
                                             NULL 
                                  FOR xml path(''''))el(br))  t 
              ON t.eventid = ea.eventid 
                 AND t.seatingid = ea.seatingid 
                 AND t.menuoptionid = ea.menuoptionid 
                 AND t.userid = ea.userid 

				 group by  ea.seatingid,es.Start, e.Start,  u.firstname , u.lastname ,am.email,u.ContactNumber,
				 ea.numberofguests,ea.menuoptionid,  emo.title,t.bookingrequirements

ORDER  BY 5, 
          3 '
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
