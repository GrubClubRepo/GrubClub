-- Author:	Swati Agrawal
-- Summary:	This script creates UserInvitee table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.10.01.CreateUserInviteeTable'

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

CREATE TABLE [dbo].[UserInvitee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[SubscriberId] [int] NOT NULL,
 CONSTRAINT [PK_UserInvitee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[UserInvitee]  WITH CHECK ADD  CONSTRAINT [FK_UserInvitee_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User]([Id])
ALTER TABLE [dbo].[UserInvitee] CHECK CONSTRAINT [FK_UserInvitee_User]

ALTER TABLE dbo.Subscriber ADD CONSTRAINT
	PK_Subscriber PRIMARY KEY CLUSTERED 
	(Id) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo.Subscriber SET (LOCK_ESCALATION = TABLE)

ALTER TABLE [dbo].[UserInvitee]  WITH CHECK ADD  CONSTRAINT [FK_UserInvitee_Subscriber] FOREIGN KEY([SubscriberId])
REFERENCES [dbo].[Subscriber]([Id])
ALTER TABLE [dbo].[UserInvitee] CHECK CONSTRAINT [FK_UserInvitee_Subscriber]

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



