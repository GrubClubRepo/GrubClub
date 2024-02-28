-- Author:	Swati Agrawal
-- Summary:	This script creates new functions and updates event report 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.13.CreateFunctionsUpdateEventReport'

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
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[StarSeperatedCuisines](@EventId INT) returns varchar(Max)
AS  
BEGIN   
DECLARE @CommaSeperatedValues VARCHAR(MAX)
SELECT @CommaSeperatedValues = COALESCE(@CommaSeperatedValues+'' * '' , '''') + C.Name
FROM EventCuisine EC WITH(NOLOCK) INNER JOIN Cuisine C WITH(NOLOCK) ON EC.CuisineId = C.Id WHERE EventId = @EventId
RETURN @CommaSeperatedValues
END  '
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[StarSeperatedTags](@EventId INT) returns varchar(Max)
AS  
BEGIN   
DECLARE @CommaSeperatedValues VARCHAR(MAX)
SELECT @CommaSeperatedValues = COALESCE(@CommaSeperatedValues+'' * '' , '''') + T.Name
FROM EventTag ET WITH(NOLOCK) INNER JOIN Tag T WITH(NOLOCK) ON ET.TagId = T.Id WHERE EventId = @EventId
RETURN @CommaSeperatedValues
END'
UPDATE [Report] SET [Query] = 'SELECT e.*, isnull([dbo].[StarSeperatedCuisines](e.ID), '' '') AS Cuisine, isnull([dbo].[StarSeperatedTags](e.ID),'' '') AS Tag
  FROM [Event] e with(nolock) 
  left join eventcuisine ec with(nolock) on e.id=ec.eventid 
  left join eventtag et with(nolock) on e.id=et.eventid 
  WHERE e.Id = @EventId' WHERE id=8 and reporttype=1 and name ='Event Details'
 UPDATE [Report] SET [Query] = 'SELECT e.*, isnull([dbo].[StarSeperatedCuisines](e.ID), '' '') AS Cuisine, isnull([dbo].[StarSeperatedTags](e.ID),'' '') AS Tag
  FROM [Event] e with(nolock) 
  left join eventcuisine ec with(nolock) on e.id=ec.eventid 
  left join eventtag et with(nolock) on e.id=et.eventid 
  WHERE e.Start BETWEEN @StartDate AND @EndDate', [parameters]='@StartDate=01-01-2014;@EndDate=31-12-2014' WHERE id=7 and reporttype=1 and name ='Events List'
 
 UPDATE [Report] SET [parameters]='@StartDate=01-01-2014;@EndDate=31-12-2014' WHERE id=19 and reporttype=4 and name ='Revenue Summary Report'
 UPDATE [Report] SET [parameters]='@StartDate=2014-01-01;@EndDate=2014-12-31' WHERE id=15 and reporttype=6 and name ='SagePay Transaction Details'


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

