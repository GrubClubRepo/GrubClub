-- Author:	Swati Agrawal
-- Summary:	Updated Report logic to show phone numbers

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.10.10.UpdateHostGuestListDownloadReport'

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
       END  AS [EventTime], 
       u.firstname + '' '' + u.lastname    AS [Name], 
       am.email, 
       isnull(u.ContactNumber,'''') AS ContactNumber,
       numberofguests                    AS NumberOfGuests, 
       CASE 
         WHEN ea.menuoptionid = 0 THEN '''' 
         ELSE emo.title 
       END                               AS MenuOptionChosen, 
       Isnull(t.bookingrequirements, '''') AS BookingRequirements 
FROM   (SELECT userid, 
               eventid, 
               seatingid, 
               menuoptionid, 
               Sum(numberofguests) AS numberofguests 
        FROM   eventattendee 
        WHERE  eventid = @EventId 
        GROUP  BY userid, 
                  eventid, 
                  seatingid, 
                  menuoptionid)ea 
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
       LEFT JOIN (select distinct t.eventid, 
							 t.seatingid, 
							 t.menuoptionid, 
							 t.userid, 
							 isnull(el.br,'''') AS bookingrequirements
					from (select eventid, 
							 seatingid, 
							 menuoptionid, 
							 userid, basketid from ticket where eventid=@EventId)t
							 cross apply (SELECT bookingrequirements + '',  '' AS [text()]
								 FROM ticketbasket tb
								 WHERE tb.userid=t.userid and tb.bookingrequirements is not null
								 FOR XML PATH(''''))el(br)) t 
              ON t.eventid = ea.eventid 
                 AND t.seatingid = ea.seatingid 
                 AND t.menuoptionid = ea.menuoptionid 
                 AND t.userid = ea.userid 
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
