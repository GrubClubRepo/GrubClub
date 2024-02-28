-- Author:	Swati
-- Summary:	Added logic to filter rows for price change before event approval

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.05.06.UpdateEventCommissionChangeLogReport'

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
set query='SELECT eventid, 
       NAME AS EventName, 
       username, 
       oldcommission, 
       newcommission, 
       date, 
       oldprice, 
       newprice, 
       action, 
       multimenuevent, 
       menuoption 
FROM   ((SELECT l.eventid, 
                e.NAME, 
                u.firstname + '' '' + u.lastname AS UserName, 
                l.oldcommission, 
                l.newcommission, 
                l.date, 
                Cast(Round((( l.oldcost + ( l.oldcost * l.oldcommission / 100 ) 
                            )), 2) 
                     AS 
                     NUMERIC(36, 2))           AS OldPrice, 
                Cast(Round((( l.newcost + ( l.newcost * l.newcommission / 100 ) 
                            )), 2) 
                     AS 
                     NUMERIC(36, 2))           AS NewPrice, 
                ''Commission Change''            AS Action, 
                CASE 
                  WHEN e.multimenuoption = 1 THEN ''Yes'' 
                  ELSE ''No'' 
                END                            AS MultiMenuEvent, 
                ''''                             AS MenuOption 
         FROM   eventcommissionchangelog l with(nolock)
                INNER JOIN [user] u with(nolock) 
                        ON l.userid = u.id 
                INNER JOIN [event] e with(nolock)
                        ON l.eventid = e.id
				WHERE e.status=1 ) 
        UNION ALL 
        (SELECT l.eventid, 
                e.NAME, 
                u.firstname + '' '' + u.lastname AS UserName, 
                NULL                           AS OldCommission, 
                NULL                           AS NewCommission, 
                l.date, 
                l.oldprice, 
                l.newprice, 
                ''Price Change''                 AS Action, 
                CASE 
                  WHEN e.multimenuoption = 1 THEN ''Yes'' 
                  ELSE ''No'' 
                END                            AS MultiMenuEvent, 
                CASE 
                  WHEN l.menuoptionid > 0 THEN emo.title 
                  ELSE '''' 
                END                            AS MenuOption 
         FROM   [dbo].[eventpricechangelog] l with(nolock)
                INNER JOIN [user] u with(nolock) 
                        ON l.userid = u.id 
                INNER JOIN [event] e with(nolock) 
                        ON l.eventid = e.id 
                LEFT JOIN [dbo].[eventmenuoption] emo with(nolock) 
                       ON emo.id = l.menuoptionid
				LEFT JOIN (select l.eventid, min(ea.bookingdate) AS BookingDate FROM [dbo].[eventpricechangelog] l with(nolock) inner join [dbo].[EventAttendee] ea with(nolock)
					  ON l.eventid = ea.eventId group by l.eventid) ea on l.eventid = ea.eventId
				LEFT JOIN (SELECT l.eventid, max(l.date) AS LastUpdateDate FROM [dbo].[eventpricechangelog] l with(nolock) 
									left join [dbo].[EventAttendee] ea with(nolock)  ON l.eventid = ea.eventId 
									inner join [event] e with(nolock) ON l.eventid = e.id where e.status=1 and e.dateapproved is null and ea.bookingdate is null  group by l.eventid)la on l.eventid = la.eventId
				WHERE e.status=1 and l.date >= isnull(e.dateapproved, isnull(ea.bookingdate,la.LastUpdateDate))))a 
ORDER  BY eventid, 
          date'
where id=42 and 
name='Event Commission Change Log Report'

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
