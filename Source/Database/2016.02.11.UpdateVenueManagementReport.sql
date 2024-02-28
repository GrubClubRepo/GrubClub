-- Author:	Supraja
-- Summary:	Updated Cost based on Event Commission

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.11.UpdateVenueManagementReport'

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
set query='
SELECT e.id, 
       Replace(e.NAME, ''"'', '''')                             AS NAME, 
       e.[address], 
       e.address2, 
       e.city, 
       e.[postcode], 
	  
	    CASE e.[HomeEvent] 
         WHEN 0 THEN ''NO'' 
         ELSE ''YES'' 
       END                                                 AS HomeEvent ,
       (CONVERT(VARCHAR(10), e.start, 103))               EventDates, 
       u.firstname + '' '' + u.lastname                       AS ChefName, 
       s.NAME                                               AS SupperclubName, 
       CASE e.[alcohol] 
         WHEN 0 THEN ''NO'' 
         ELSE ''YES'' 
       END                                                  AS BYOB, 
       CASE WHEN e.[multiseating] = 1 and es.eventid is not null
         THEN es.[guests] 
         ELSE e.guests 
       END                                                  guests, 
       Cast(Round((( e.cost+(e.cost * e.commission/100) )), 2) AS NUMERIC(36, 2)) AS price 
FROM   [event] e 
       JOIN supperclub s 
         ON e.supperclubid = s.id 
       JOIN [user] u 
         ON s.userid = u.id 
       LEFT JOIN (select eventId, sum(Guests) as Guests from [eventseating] group by eventid) es 
              ON e.id = es.[eventid] 
WHERE  e.start >= @sdate
       AND e.[end] <= @edate 
       AND e.status = 1 
ORDER  BY e.start  '
where id='26' and name='Venue Management'

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