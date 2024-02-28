-- Author:	Swati Agrawal
-- Summary:	This adds the Event Menu Option & Event Seating Table, and adds columns in EventAttendee, Ticket and Event table 

DECLARE @ScriptCode nvarchar(250)
SET @ScriptCode='2013.06.24.AddEventMenuOptionSeatingTableAndColumns'

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
CREATE TABLE [dbo].[EventSeating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[Start] [datetime] NOT NULL,
	[End] [datetime] NOT NULL,
	[Guests] [int] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[ReservedSeats] [int] NOT NULL,
CONSTRAINT [PK_EventSeating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

ALTER TABLE [dbo].[EventSeating]  WITH CHECK ADD CONSTRAINT [FK_EventSeating_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([Id])

ALTER TABLE [dbo].[EventSeating] CHECK CONSTRAINT [FK_EventSeating_Event]

ALTER TABLE [dbo].[EventSeating] ADD  CONSTRAINT [DF_EventSeating_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]

ALTER TABLE [dbo].[EventSeating] ADD  CONSTRAINT [DF_EventSeating_IsDefault]  DEFAULT (0) FOR [IsDefault]

CREATE TABLE [dbo].[EventMenuOption](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[EventId] [int] NOT NULL,
	[Cost] [money] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_EventMenuOption] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



ALTER TABLE [dbo].[EventMenuOption]  WITH CHECK ADD CONSTRAINT [FK_EventMenuOption_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([Id])


ALTER TABLE [dbo].[EventMenuOption] CHECK CONSTRAINT [FK_EventMenuOption_Event]


ALTER TABLE [dbo].[EventMenuOption] ADD  CONSTRAINT [DF_EventMenuOption_DateCreated]  DEFAULT (GETDATE()) FOR [DateCreated]


ALTER TABLE [dbo].[EventMenuOption] ADD  CONSTRAINT [DF_EventMenuOption_IsDefault]  DEFAULT (0) FOR [IsDefault]

SET IDENTITY_INSERT EventSeating ON
INSERT INTO [SupperClub].[dbo].[EventSeating]([Id],[EventId],[Start],[End],[Guests],[DateCreated],[ReservedSeats],[IsDefault])
     VALUES (0,2,'1900-01-01 00:00:00.000','1900-01-01 02:00:00.000',0,GETDATE(),0,0)
SET IDENTITY_INSERT EventSeating OFF
    
     
SET IDENTITY_INSERT EventMenuOption ON         
INSERT INTO [SupperClub].[dbo].[EventMenuOption]([Id],[EventId],[Title],[Description],[Cost],[DateCreated],[IsDefault])
     VALUES (0,2,'','',0,GETDATE(),0)
SET IDENTITY_INSERT EventMenuOption OFF



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventAttendee'
AND COLUMN_NAME = 'SeatingId')
ALTER TABLE [DBO].[EventAttendee] ADD SeatingId INT NOT NULL DeFAULT (0)

ALTER TABLE [dbo].[EventAttendee]  WITH CHECK ADD CONSTRAINT [FK_EventAttendees_EventSeating] FOREIGN KEY([SeatingId])
REFERENCES [dbo].[EventSeating] ([Id])

ALTER TABLE [dbo].[EventAttendee] CHECK CONSTRAINT [FK_EventAttendees_EventSeating]



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventAttendee'
AND COLUMN_NAME = 'MenuOptionId')
ALTER TABLE [DBO].[EventAttendee] ADD MenuOptionId INT NOT NULL DeFAULT (0)

ALTER TABLE [dbo].[EventAttendee]  WITH CHECK ADD  CONSTRAINT [FK_EventAttendees_EventMenuOption] FOREIGN KEY([MenuOptionId])
REFERENCES [dbo].[EventMenuOption] ([Id])

ALTER TABLE [dbo].[EventAttendee] CHECK CONSTRAINT [FK_EventAttendees_EventMenuOption]



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Ticket'
AND COLUMN_NAME = 'SeatingId')
ALTER TABLE [DBO].[Ticket] ADD SeatingId int NOT NULL DEFAULT (0)

ALTER TABLE [dbo].[Ticket]  WITH CHECK ADD CONSTRAINT [FK_Ticket_EventSeating] FOREIGN KEY([SeatingId])
REFERENCES [dbo].[EventSeating] ([Id])

ALTER TABLE [dbo].[Ticket] CHECK CONSTRAINT [FK_Ticket_EventSeating]



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Ticket'
AND COLUMN_NAME = 'MenuOptionId')
ALTER TABLE [DBO].[Ticket] ADD MenuOptionId int NOT NULL DEFAULT (0)

ALTER TABLE [dbo].[Ticket]  WITH CHECK ADD  CONSTRAINT [FK_Ticket_EventMenuOption] FOREIGN KEY([MenuOptionId])
REFERENCES [dbo].[EventMenuOption] ([Id])

ALTER TABLE [dbo].[Ticket] CHECK CONSTRAINT [FK_Ticket_EventMenuOption]


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'MultiSeating')
ALTER TABLE [DBO].[Event] ADD MultiSeating BIT NOT NULL CONSTRAINT [DF_Event_MultiSeating] DEFAULT (0)


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'MultiMenuOption')
ALTER TABLE [DBO].[Event] ADD MultiMenuOption BIT NOT NULL CONSTRAINT [DF_Event_MultiMenuOption] DEFAULT (0)

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


