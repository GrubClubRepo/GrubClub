-- Author:	Swati
-- Summary:	Added new columns to the report

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.03.23.UpdateUpcomingGrubClubEventsReport'

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

CREATE TABLE #SupperClubList (SupperClubId INT, EventId INT)
DECLARE @NumOfChefs INT
IF @EndDate < GETDATE()
BEGIN
	INSERT INTO #SupperClubList(SupperClubId, EventId)
	SELECT distinct SupperClubId, eventId 
	FROM dbo.EventAttendee ea with(nolock) 
	INNER JOIN dbo.Event e with(nolock)  on ea.eventid = e.id
	where e.start between @StartDate and @EndDate
END
ELSE
BEGIN
	INSERT INTO #SupperClubList(SupperClubId, EventId)
	SELECT SupperClubId, Id AS EventId
	FROM dbo.Event e with(nolock) 
	where (e.start between @StartDate and @EndDate) and status=1
END
SELECT @NumOfChefs = COUNT(distinct SupperClubId) FROM #SupperClubList

SELECT e.name                         AS EventName, 
       s.name                         AS GrubClub, 
       isnull(u.firstname,'''') + '' '' + isnull(u.lastname,'''') AS HostedBy,
       e.start                        AS EventDate, 
       Cast(Round(e.cost * (1+ (e.commission/100.0)), 2) AS NUMERIC(36, 2)) AS Price, 
	   CASE 
         WHEN e.multiseating = 0 THEN e.guests 
         ELSE es.guests 
       END                            AS TotalSeatsAvailable, 
       Sum(Isnull(numberofguests, 0)) AS SeatsBooked, 
       CASE 
         WHEN e.multiseating = 0 THEN e.reservedseats 
         ELSE es.reservedseats 
       END                            AS ReservedSeats, 
       CASE 
         WHEN Sum(Isnull(numberofguests, 0)) + ( CASE 
                     WHEN e.multiseating = 0 THEN e.reservedseats 
                     ELSE es.reservedseats 
                                                 END ) >= ( CASE 
                     WHEN e.multiseating = 0 THEN e.guests 
                     ELSE es.guests 
                                                            END ) THEN ''SOLDOUT'' 
         ELSE '''' 
       END                            EventStatus, 
       CASE 
         WHEN e.multiseating = 0 THEN ''Single'' 
         ELSE ''Multi Seating'' 
       END                            AS SeatingType, 
       CASE 
         WHEN e.[private] = 0 THEN ''Public'' 
         ELSE ''Private'' 
       END                            EventType,
	   @NumOfChefs AS NumberOfChefs,
	   EventCount AS NumberOfEventsOfThisChef,
	   RegistrationDate,	   
	   CASE WHEN e.multiseating = 0 THEN e.Guests 
         ELSE es.Guests
       END * (Cast(Round(e.cost * (1+ (e.commission/100.0)), 2) AS NUMERIC(36, 2)))    AS ProjectedRevenue,
	   isnull(sum(ea.[GuestBasePrice]),0) AS ActualRevenue              
FROM   #SupperClubList scl 
	   INNER JOIN dbo.[event] e WITH(nolock) 
			   ON e.Id = scl.EventId
       INNER JOIN [supperclub] s WITH(nolock) 
               ON e.supperclubid = s.id 
       INNER JOIN [user] u WITH(nolock) 
               ON s.userid = u.id
	   INNER JOIN (SELECT SupperClubId, Count(EventId) AS EventCount FROM #SupperClubList group by SupperClubId)eCount
			   ON eCount.SupperClubId = scl.SupperClubId
	   INNER JOIN (SELECT e.SupperClubId, min(e.start) AS RegistrationDate
					FROM #SupperClubList scl 
					INNER JOIN dbo.Event e with(nolock)  on scl.SupperClubId = e.SupperClubId
					group by e.SupperClubId)er ON er.SupperClubId = scl.SupperClubId
       LEFT JOIN [eventattendee] ea WITH(nolock) 
              ON ea.eventid = e.id 
       LEFT JOIN (SELECT eventid, 
                         Sum(guests)        AS Guests, 
                         Sum(reservedseats) AS ReservedSeats 
                  FROM   [eventseating] WITH(nolock) 
                  GROUP  BY eventid)es 
              ON es.eventid = e.id 
GROUP  BY e.name, 
          s.name,
          u.firstname,
          u.LastName, 
          e.start, 
          e.cost, 
          e.multiseating, 
          e.guests, 
          es.guests, 
          e.reservedseats, 
          es.reservedseats, 
          e.[private],
		  e.Commission,
		  EventCount,
	      RegistrationDate
ORDER  BY s.name ASC 

DROP TABLE #SupperClubList'
where id='21' and name='Upcoming Grubclub Events List'

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

