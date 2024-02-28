-- Author:	Swati
-- Summary:	Added new report to show new hosts who registered in last 7 days

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.04.05.AddNewlyRegisteredHostReport'

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
INSERT INTO [dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[ReportType]
           ,[Query]
           )
     VALUES
           ('Newly Registered Host Report'
           ,null
           ,2
           ,N'select s.name AS ProfileName, 
asp.Email,
u.firstname + '' '' + u.lastname AS ChefName,
u.contactNumber,
s.address + case when (s.address2 is not null and s.address2 !='''') then '','' + s.address2 else '''' end as Address, 
s.city, 
s.postcode, 
NumberOfEventsCreated, 
convert(date, StartDate) AS RegistrationDate,
ev.name as EventName,
ev.start as EventDate,
(Cast(dbo.RoundBanker(ev.cost * (1+ (ev.commission/100.0)), 2) AS NUMERIC(36, 2))) AS PricePerTicket,
''https://grubclub.com/'' + s.Urlfriendlyname + ''/'' + ev.Urlfriendlyname + ''/'' + convert(varchar(10), ev.Id) AS EventLink
from supperclub s with(nolock) 
inner join [user] u with(nolock) on s.userid = u.Id
inner join aspnet_Membership asp with(nolock) on asp.userid = u.id
inner join (select supperclubid, count(id) AS NumberOfEventsCreated, min(DateCreated) as StartDate from event  with(nolock) 
			group by supperclubid 
			having (DATEDIFF(day,  DATEADD(day, DATEDIFF(day, 0,min(DateCreated)), 0), DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)) <= 7))e on s.id= e.supperclubid
inner join [event] ev with(nolock) on s.id= ev.supperclubid')

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

