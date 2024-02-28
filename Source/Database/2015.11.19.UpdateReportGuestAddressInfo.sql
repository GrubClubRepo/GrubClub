-- Author:	Swati Agrawal
-- Summary:	This updates report for guest's address info

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.19.UpdateReportGuestAddressInfo'

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
SET Query= N'select Isnull(u.firstname, '''') AS FirstName,
       Isnull(u.lastname, '''')  AS LastName, 
       ISNULL(u.Gender,'''') AS Gender,
asp.Email,
isnull(u.address,'''') AS Address,
isnull(u.postcode,'''') AS Postcode,
case when address IS null then '''' else CASE when CHARINDEX('','', address) = 0 THEN address ELSE RIGHT(address,CHARINDEX('','',REVERSE(address)) - 1) end end as City,
isnull(u.country,'''') AS Country,
isnull(u.contactnumber,'''') AS ContactNumber,
ea.NumberOfEvents,
ea.FirstBookingDate,
ea.LastBookingDate
from dbo.[user] u with(nolock)
inner join dbo.aspnet_membership asp with(nolock) on u.id=asp.userid
inner join (select userid, count(distinct eventid) As NumberOfEvents, min(bookingdate) as FirstBookingDate, max(bookingdate) as LastBookingDate from dbo.[eventattendee] with(nolock) where bookingdate is not null group by userid) ea  on ea.userid=u.id
where (u.address is not null OR u.postcode is not null)
order by FirstName
'
WHERE Name= 'Guest Address Info Report' AND ReportType=3

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


