-- Author:	Swati Agrawal
-- Summary:	added voucher info to report

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.07.17.UpdateEventMasterReport'

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
SET Query='SELECT e.name                             AS EventName, 
       e.start                            AS EventDate, 
       a.email                            AS Email, 
       u.firstname, 
       u.lastname, 
       ( u.firstname + '' '' + u.lastname ) AS GuestName, 
       Round(CASE 
               WHEN e.multimenuoption = 1 THEN emo.cost 
               ELSE e.cost 
             END * 1.1, 2)                AS [PricePerTicket: This is the price per ticket which we display on the website to the users],        
       NumberOfGuests AS [NumberOfGuests: Number of tickets this guest has booked], 
       ea.costtochef                      AS [AmountDueToChef:(Number of tickets * Per ticket base price i.e without GC commission)-(any discount using chef voucher)], 
       ea.costtoguest                     AS [CostToGuest: (Number of tickets * Price per ticket) - Discount], 
       Isnull(emo.title, '''')              AS MenuOption,
       CASE WHEN ea.VoucherId=0 then '''' else case when ea.AdminVoucherCode = 1 THEN ''Admin'' ELSE ''Chef'' END end AS VocuherIssuedBy,
       ISNULL(v.Description, '''') AS VoucherDescription
FROM   dbo.[event] e WITH(nolock) 
       INNER JOIN (SELECT i.eventid, 
                          i.userid, 
                          i.menuoptionid,
                          i.AdminVoucherCode, 
                          NumberOfGuests, 
                          CostToGuest, 
                          CostToChef,
                          MAX(t.voucherId) as VoucherId
                          from (SELECT e.eventid, 
                          e.userid, 
                          e.menuoptionid,
                          e.AdminVoucherCode, 
                          sum(numberofguests) AS NumberOfGuests, 
                          Sum(e.totalprice)     AS CostToGuest, 
                          Sum(hostnetprice)   AS CostToChef                           
                   FROM   dbo.eventattendee e WITH(nolock) 
                   WHERE  e.eventid = @EventId 
                   GROUP  BY e.eventid, 
                          e.userid, 
                          e.menuoptionid,
                          e.AdminVoucherCode)i
                          inner join dbo.Ticket t WITH(nolock) on i.EventId=t.EventId and i.UserId = t.UserId and i.MenuOptionId = t.MenuOptionId
                          group by i.eventid, 
                          i.userid, 
                          i.menuoptionid,
                          i.AdminVoucherCode, 
                          NumberOfGuests, 
                          CostToGuest, 
                          CostToChef)ea
                    
               ON e.id = ea.eventid 
       INNER JOIN dbo.[user] u WITH(nolock) 
               ON u.id = ea.userid 
       INNER JOIN dbo.aspnet_membership a WITH(nolock) 
               ON a.userid = u.id 
       LEFT JOIN [eventmenuoption] emo 
              ON emo.id = ea.menuoptionid 
       LEFT JOIN [voucher] v 
              ON v.id = ea.VoucherId 
WHERE  e.id = @EventId 
ORDER  BY firstname ASC '
WHERE Name= 'Event Master' AND [Parameters]='@EventId=1' AND ReportType=1

	
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
