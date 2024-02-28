-- Author:	Supraja
-- Summary:	Updated Cost with Commission Value

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.04.1.UpdateGetUserEventListForUpcomingWishListedEvents'

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

	EXEC sp_executesql N'alter PROCEDURE [dbo].[GetUserEventListForUpcomingWishListedEvents]  
@Offset int                                           
AS                                                      
BEGIN                                                      
SET NOCOUNT ON; 
CREATE TABLE #EVENT_LIST(UserId uniqueidentifier, EventId int, Name nvarchar(500), Start DateTime)

insert into #EVENT_LIST(UserId, EventId, Name, Start)                                                     
select distinct u.id AS UserId,  
e.Id AS EventId,  
e.name AS EventName,  
e.start  
from dbo.[Event] e with(readuncommitted)  
inner join dbo.UserFavouriteEvent ufe with(readuncommitted) on e.Id = ufe.EventId  
inner join dbo.[User] u with(readuncommitted) on u.Id = ufe.UserId  
inner join dbo.SupperClub s with(readuncommitted) on e.SupperClubId = s.Id  
left join dbo.EventAttendee ea with(readuncommitted) on ea.UserId = ufe.UserId and ea.EventId = ufe.EventId  
left join (select eventid, sum(guests) as Guests, sum(ReservedSeats) as ReservedSeats from dbo.EventSeating with(readuncommitted) group by EventId) es on es.EventId=e.Id  
left join (select eventid, sum(NumberOfGuests) as NumberOfGuests from dbo.EventAttendee with(readuncommitted) group by EventId) eal on eal.EventId=e.Id  
where s.Active=1 and e.Status = 1 and e.start > getdate() and ufe.EmailNotificationSent = 0 and ea.id is null and DATEDIFF(day, DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0), DATEADD(day, DATEDIFF(day, 0, e.start), 0)) <= 30  
and ((case when es.EventId is null then e.Guests else es.Guests end) > ((case when es.EventId is null then e.ReservedSeats else es.ReservedSeats end) + (case when eal.EventId IS null then 0 else eal.NumberOfGuests End)))  
    
select el.Userid AS UserId,      
u.firstname,      
asp.email,      
el.EventId AS EventId,      
el.Name AS EventName,      
el.start,  
e.[end],  
e.cost,  
e.Commission,
e.city,  
e.UrlFriendlyName,  
s.UrlFriendlyName AS SupperClubUrlFriendlyName  
from (select UserId, Name, MIN(Start) AS Start from #EVENT_LIST group by UserId, Name) dl
inner join #EVENT_LIST el on el.UserId = dl.UserId and el.Name = dl.Name and el.Start = dl.Start
inner join dbo.[Event] e with(readuncommitted) on  e.id=el.eventid
inner join dbo.[User] u with(readuncommitted) on u.Id = el.UserId  
inner join dbo.aspnet_Membership asp with(readuncommitted) on asp.UserId = u.Id  
inner join dbo.SupperClub s with(readuncommitted) on e.SupperClubId = s.Id  
order by u.id  
DROP TABLE #EVENT_LIST 
  END '

	

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