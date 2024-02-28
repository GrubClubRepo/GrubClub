-- Author:	Swati Agrawal
-- Summary:	Fixed the where condition to return accurate data

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.25.UpdateVenueManagementReport'

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
SET Query=N'SELECT e.id, 
       Replace(e.NAME, ''"'', '''')                             AS NAME, 
       e.[address], 
       e.address2, 
       e.city, 
       e.[postcode], 
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
       Cast(Round((( e.cost * 1.1 )), 2) AS NUMERIC(36, 2)) AS price 
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
ORDER  BY e.start '
WHERE Name= 'Venue Management' AND ReportType=5

	
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
