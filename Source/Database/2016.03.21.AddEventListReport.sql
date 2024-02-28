-- Author:	Swati Agrawal
-- Summary:	This returns all events during a defined time period

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.03.21.AddEventListReport'

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
           ('Tom''s Event List Report'
           ,'@StartDate=23-03-2016;@EndDate=31-12-2016'
           ,5
           ,N'select e.name as Title,
CONVERT(date, start) as Date,
CONVERT(VARCHAR(5),start,108) + ''-'' + CONVERT(VARCHAR(5),[end],108) AS [Time],
e.address + (case when (e.address2 is null OR e.address2 = '''') then '''' else '','' + e.Address2 end) + '','' + e.City AS Address,
e.Postcode,
[dbo].[fn_StripCharacters](e.Description) AS Description,
substring([dbo].[fn_StripCharacters](e.Description), 0,230) AS ShortDetails,
''http://grubclub.com/'' + s.UrlFriendlyName + ''/'' + e.UrlFriendlyName AS Link
from dbo.Event e with(nolock)
inner join dbo.SupperClub s with(nolock) on e.SupperClubId = s.Id
where e.status=1 and s.active=1 and (e.start between @StartDate and @EndDate) 
order by start')
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





