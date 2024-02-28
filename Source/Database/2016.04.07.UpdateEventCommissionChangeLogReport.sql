-- Author:	Swati
-- Summary:	Added new columns to the report

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.04.07.UpdateEventCommissionChangeLogReport'

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


select EventId, 
Name AS EventName, 
UserName,
OldCommission,
NewCommission,
Date,
OldPrice,
NewPrice,
Action,
MultiMenuEvent,
MenuOption
from (
(select l.EventId,e.Name,
u.FirstName+'' ''+u.LastName as UserName,
l.OldCommission,
l.NewCommission,
l.Date,
Cast(Round((( l.oldcost+(l.oldcost * l.OldCommission/100) )), 2) AS NUMERIC(36, 2)) AS OldPrice,
Cast(Round((( l.newcost+(l.newcost * l.NewCommission/100) )), 2) AS NUMERIC(36, 2)) AS NewPrice,
''Commission Change'' AS Action,
case when e.MultiMenuOption =1 then ''Yes'' else ''No'' end AS MultiMenuEvent,
'''' AS MenuOption
from eventcommissionchangelog l
inner join [user] u on l.userid=u.id
inner join [Event] e on l.eventid=e.id)
union all
(select l.EventId,e.Name,u.FirstName+'' ''+u.LastName as UserName,
null as OldCommission,
null as NewCommission,
l.Date,
l.OldPrice,
l.NewPrice,
''Price Change'' AS Action,
case when e.MultiMenuOption =1 then ''Yes'' else ''No'' end AS MultiMenuEvent,
case when l.menuoptionid > 0 then emo.title else '''' end AS MenuOption
from [dbo].[EventPriceChangeLog] l
inner join [user] u on l.userid=u.id
inner join [Event] e on l.eventid=e.id
left join [dbo].[EventMenuOption] emo on emo.Id = l.menuoptionid))a
order by eventid,date'
where id=42 and name='Event Commission Change Log Report'

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

