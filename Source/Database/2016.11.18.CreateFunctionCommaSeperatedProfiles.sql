-- Author:	Swati Agrawal
-- Summary:	This script creates new function

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.11.18.CreateFunctionCommaSeperatedProfiles'

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
CREATE FUNCTION [dbo].[CommaSeperatedProfiles](@TagId INT) returns varchar(Max)
AS  
BEGIN   
DECLARE @CommaSeperatedValues VARCHAR(MAX)
SELECT @CommaSeperatedValues = COALESCE(@CommaSeperatedValues+'','' , '''') +  a.Name
FROM 
(Select distinct S.Name FROM
dbo.EventTag ET WITH(NOLOCK) 
INNER JOIN dbo.Event E WITH(NOLOCK) ON ET.EventId = E.Id 
INNER JOIN dbo.SupperClub S WITH(NOLOCK) ON S.Id = E.SupperClubId
INNER JOIN (Select distinct EventId from dbo.EventAttendee WITH(NOLOCK)) EA ON EA.EventId = E.Id
WHERE TagId = @TagId)a
RETURN @CommaSeperatedValues
END'
 


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

