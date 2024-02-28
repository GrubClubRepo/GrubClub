-- Author:	Josh Helmink
-- Summary:	This inserts the system Email Templates

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2012.11.01.Inserting_EmailTemplates'

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

--ContactUsForm = 1,
--PasswordReset = 2,
--SupperClubRegistered = 3,
--BookingConfirmedGuest = 4,
--BookingConfirmedHost = 5,
--GuestRefused = 6,
--EventCancelled = 7,

exec sp_executesql
N'
INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           1,''GrubClub Contact Us Form Message'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Message from: <userName> <emailAddress> : <time>, Message reads: <message></body></html>'', ''True'')

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           2,''GrubClub Password Reset'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Welcome <userName>, <br/><br/>Your new password is: <newPassword><br/><br/>Click <a href="<ServerURL>">here</a> to login.<br/><br/>Regards,<br/>The GrubClub Team</body></html>'', ''True'')
           
INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           3,''New SupperClub Registered'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>New SupperClub Registered</body></html>'', ''True'')

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           4,''GrubClub Booking Confirmation'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Guest Booking Confirmation</body></html>'', ''True'')

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           5,''GrubClub Booking Confirmation'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Host Booking Confirmation</body></html>'', ''True'')

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           6,''GrubClub Guest Refused'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Guest Refused</body></html>'', ''True'')

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id]
           ,[Subject]
           ,[Body]
           ,[Html])
     VALUES
           (
           7,''GrubClub Event Cancelled'', ''<html><body><img alt="GrubClub logo" src="<ServerURL>/Content/images/logo.png"<br/><br/><br/>Event Cancelled</body></html>'', ''True'')           
'

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