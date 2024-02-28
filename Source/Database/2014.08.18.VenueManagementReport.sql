-- Author:	Supraja
-- Summary:	This returns reviews for all events during a defined time period

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.07.23.Venue Management'

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
           ('Venue Management'
           ,'@sdate=1-12-2013;@edate=1-2-2014'
           ,5
           ,N'select e.id, REPLACE(e.Name,''"'','''') as Name, e.[Address],e.Address2 , e.city,e.[PostCode],
(CONVERT(VARCHAR(10), e.start, 103))   EventDates,
u.FirstName+ '' ''+ u.LastName as ChefName,s.Name as SupperclubName,
case e.[Alcohol] when 0 then ''NO'' ELSE ''YES'' END as BYOB
,case e.[MultiSeating] when   0 then  e.[Guests]
     ELSE sum(es.Guests)  end  guests,
cast(Round(((e.Cost*1.1)),2)  as numeric(36,2)) as price
 from [Event] e
 join  supperclub s on e.SupperClubId=s.id
 join  [User] u on s.UserId=u.Id
left join  [EventSeating]  es on e.id=es.[EventId]
where e.start>=@sdate and  e.[end]<=@edate  and e.Active=1
group by e.id,e.Name,e.Address,e.Address2, e.City,e.[PostCode],e.start,e.[End],u.FirstName,u.LastName, e.Alcohol,e.Guests
,e.Cost,e.[MultiSeating],s.Name

order by e.Start')

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