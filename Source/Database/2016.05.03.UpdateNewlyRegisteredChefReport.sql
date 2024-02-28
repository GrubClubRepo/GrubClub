-- Author:	Swati
-- Summary:	Added logic to show chefs with zero events

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.05.03.UpdateNewlyRegisteredChefReport'

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
CREATE TABLE #SupperClubList(SupperClubId int, NumOfEvents int, DateCreated datetime)
insert into #SupperClubList(SupperClubId, NumOfEvents, DateCreated)
SELECT supperclubid, 
                          Count(id)        AS NumberOfEventsCreated, 
                          Min(datecreated) AS StartDate 
                   FROM   event WITH(nolock) 
                   GROUP  BY supperclubid 
                   HAVING Min(datecreated) BETWEEN @StartDate AND @EndDate
				   
insert into #SupperClubList(SupperClubId, NumOfEvents, DateCreated)
select s.id, 0, null 
from SupperClub s 
left join #SupperClubList scl on s.id = scl.supperclubid
left join Event e on s.id = e.supperclubid
where s.id > (select min(SupperClubId) from #SupperClubList) and scl.supperclubid is null and e.supperclubid is null

SELECT s.NAME                         AS ProfileName, 
       asp.email, 
       u.firstname + '' '' + u.lastname AS ChefName, 
       u.contactnumber, 
       s.address + CASE WHEN (s.address2 IS NOT NULL AND s.address2 !='''') THEN 
       '','' + 
       s.address2 ELSE '''' END         AS Address, 
       s.city, 
       s.postcode, 
       NumOfEvents AS numberofeventscreated, 
       CONVERT(DATE, e.DateCreated)       AS RegistrationDate, 
       isnull(ev.NAME, '''')                    AS EventName, 
       isnull(ev.start,null)                      AS EventDate, 
       case when ev.cost is null then null else( Cast(dbo.Roundbanker(ev.cost * ( 1 + ( ev.commission / 100.0 ) ), 2) AS 
                NUMERIC(36, 2)) ) end    AS PricePerTicket, 
       isnull(''https://grubclub.com/'' + s.urlfriendlyname 
       + ''/'' + ev.urlfriendlyname + ''/'' 
       + CONVERT(VARCHAR(10), ev.id),'''')  AS EventLink 
FROM   supperclub s WITH(nolock) 
       INNER JOIN [user] u WITH(nolock) 
               ON s.userid = u.id 
       INNER JOIN aspnet_membership asp WITH(nolock) 
               ON asp.userid = u.id 
       INNER JOIN #SupperClubList e 
               --(DATEDIFF(day,  DATEADD(day, DATEDIFF(day, 0,min(DateCreated)), 0), DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)) <= 7))e 
               ON s.id = e.supperclubid 
       left JOIN [event] ev WITH(nolock) 
               ON s.id = ev.supperclubid
DROP TABLE #SupperClubList'
where id=43

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

