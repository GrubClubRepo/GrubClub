-- Author:	Swati
-- Summary:	added new Columns and updated headers for consistency

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.12.01.UpdateVenueManagementReport'

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
SET Query='SELECT e.id,
		s.Name AS ProfileName, 
       Replace(e.NAME, ''"'', '''')                             AS EventName, 
       e.[address], 
       e.address2, 
       e.city, 
       e.[postcode], 
       CASE e.[homeevent] 
         WHEN 0 THEN ''NO'' 
         ELSE ''YES'' 
       END                                                  AS HomeEvent, 
       ( CONVERT(VARCHAR(10), e.start, 103) )               EventDate, 
       u.firstname + '' '' + u.lastname                       AS ChefName, 
       s.NAME                                               AS SupperclubName, 
       CASE e.[alcohol] 
         WHEN 0 THEN ''NO'' 
         ELSE ''YES'' 
       END                                                  AS BYOB,
       case when s.Active  = 1 then ''YES'' ELSE ''NO'' end AS ProfileActive,
       case when e.Status = 2 then ''YES'' ELSE ''NO'' end AS EventCancelled,        
       CASE 
         WHEN e.[multiseating] = 1 
              AND es.eventid IS NOT NULL THEN es.[guests] 
         ELSE e.guests 
       END                                                  VenueCapacity, 
       e.ReservedSeats,
       case when ea.EventId IS null then 0 else ea.SeatsSold end as SeatsSold,       
       Cast(Round((( e.cost * 1.1 )), 2) AS NUMERIC(36, 2)) AS PriceAdvertised,
       case when ea.EventId IS null then 0 else ea.GrossRevenue end as GrossRevenue,
       case when ea.EventId IS null then 0 else ea.NetRevenue end as NetRevenue
FROM   dbo.[event] e with(nolock)
       JOIN dbo.supperclub s with(nolock)
         ON e.supperclubid = s.id 
       JOIN dbo.[user] u with(nolock)
         ON s.userid = u.id 
       LEFT JOIN (SELECT eventid, 
                         Sum(guests) AS Guests 
                  FROM   dbo.[eventseating] with(nolock)
                  GROUP  BY eventid) es 
              ON e.id = es.[eventid] 
       LEFT JOIN (SELECT eventId, SUM(numberofguests) as SeatsSold, SUM(GuestBasePrice) AS GrossRevenue, SUM(TotalPrice) as NetRevenue
       from EventAttendee with(nolock) group by EventId)EA on EA.EventId = e.Id
WHERE  e.start >= @sdate 
       AND e.[end] <= @edate 
       --AND e.status = 1 
ORDER  BY e.start   '
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
