
-- Author:	Supraja
-- Summary:	Updated Cost based on Event Commission

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.11.UpdateEventMasterReport'

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
update report
set Query='

SELECT e.NAME                             AS EventName, 
       e.start                            AS EventDate, 
       a.email                            AS Email, 
       u.firstname, 
       u.lastname, 
       ( u.firstname + '' '' + u.lastname ) AS GuestName, 
       s.Name AS ChefProfileName,
       CASE WHEN e.Charity=1 THEN ''Yes'' ELSE ''No'' END AS Charity,
       CASE WHEN e.[Private]=1 THEN ''Yes'' ELSE ''No'' END AS [Private],
       Round(CASE 
               WHEN e.multimenuoption = 1 THEN (emo.cost +emo.cost*e.Commission/100)
               ELSE (e.cost +e.cost*e.Commission/100)
             END , 2)                AS 
[PricePerTicket: This is the price per ticket which we display on the website to the users] 
       , 
numberofguests                     AS 
       [NumberOfGuests: Number of tickets this guest has booked], 
ea.costtochef                      AS 
[AmountDueToChef:(Number of tickets * Per ticket base price i.e without GC commission)-(any discount using chef voucher)] 
       , 
ea.costtoguest                     AS 
[CostToGuest: (Number of tickets * Price per ticket) - Discount], 
Isnull(emo.title, '''')              AS MenuOption, 
CASE 
  WHEN ea.voucherid = 0 THEN '''' 
  ELSE 
    CASE 
      WHEN ea.adminvouchercode = 1 THEN ''Admin'' 
      ELSE ''Chef'' 
    END 
END                                AS VocuherIssuedBy, 
Isnull(v.description, '''')          AS VoucherDescription 
FROM   dbo.[event] e WITH(nolock) 
       INNER JOIN (SELECT i.eventid, 
                          i.userid, 
                          i.menuoptionid, 
                          i.adminvouchercode, 
                          numberofguests, 
                          costtoguest, 
                          costtochef, 
                          Max(t.voucherid) AS VoucherId 
                   FROM   (SELECT e.eventid, 
                                  e.userid, 
                                  e.menuoptionid, 
                                  e.adminvouchercode, 
                                  Sum(numberofguests) AS NumberOfGuests, 
                                  Sum(e.totalprice)   AS CostToGuest, 
                                  Sum(hostnetprice)   AS CostToChef 
                           FROM   dbo.eventattendee e WITH(nolock) 
                           WHERE  e.eventid = @EventId 
                           GROUP  BY e.eventid, 
                                     e.userid, 
                                     e.menuoptionid, 
                                     e.adminvouchercode)i 
                          LEFT JOIN dbo.ticket t WITH(nolock) 
                                  ON i.eventid = t.eventid 
                                     AND i.userid = t.userid 
                                     AND i.menuoptionid = t.menuoptionid 
                   GROUP  BY i.eventid, 
                             i.userid, 
                             i.menuoptionid, 
                             i.adminvouchercode, 
                             numberofguests, 
                             costtoguest, 
                             costtochef)ea 
               ON e.id = ea.eventid 
       INNER JOIN dbo.[user] u WITH(nolock) 
               ON u.id = ea.userid 
       INNER JOIN dbo.aspnet_membership a WITH(nolock) 
               ON a.userid = u.id
       INNER JOIN dbo.SupperClub s WITH(nolock) 
               ON s.id = e.SupperClubId 
       LEFT JOIN [eventmenuoption] emo 
              ON emo.id = ea.menuoptionid 
       LEFT JOIN [voucher] v 
              ON v.id = ea.voucherid 
WHERE  e.id = @EventId 
ORDER  BY firstname ASC 
'
where id=16 and name='Event Master'

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