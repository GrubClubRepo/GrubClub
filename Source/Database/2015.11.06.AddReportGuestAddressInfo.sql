-- Author:	Swati Agrawal
-- Summary:	This returns guest's address info

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.06.AddReportGuestAddressInfo'

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
           ('Guest Address Info Report'
           ,NULL
           ,3
           ,N'select isnull(u.firstname,'''') + '' '' + isnull(u.lastname,'''') as Name,
asp.Email,
(isnull(u.address,'''') AS Address,
(isnull(u.postcode,'''') AS Postcode,
(isnull(u.country,'''') AS Country,
isnull(u.contactnumber,'''') AS ContactNumber,
ea.NumberOfEvents,
ea.FirstBookingDate,
ea.LastBookingDate
from dbo.[user] u with(nolock)
inner join dbo.aspnet_membership asp with(nolock) on u.id=asp.userid
inner join (select userid, count(distinct eventid) As NumberOfEvents, min(bookingdate) as FirstBookingDate, max(bookingdate) as LastBookingDate from dbo.[eventattendee] with(nolock) where bookingdate is not null group by userid) ea  on ea.userid=u.id
where (u.address is not null OR u.postcode is not null)
order by Name
')
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


