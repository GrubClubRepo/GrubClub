-- Author:	Swati Agrawal
-- Summary:	This script creates MessageTemplate table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.29.CreateMessageTemplateTable'

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

CREATE TABLE [dbo].[MessageTemplate](
	[Id] [int] NOT NULL,
	[MessageTemplateType] [varchar](50) NOT NULL,
	[AlertText] [nvarchar](256) NOT NULL,
	[MessageBody] [nvarchar](1028) NOT NULL,
 CONSTRAINT [PK_MessageTemplates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

EXEC sp_executesql N'INSERT INTO [MessageTemplate](Id, MessageTemplateType, AlertText, MessageBody) VALUES(1, ''FB_FRIEND_INSTALLED_APP'', ''Your friend [friendName] has also installed the GrubClub App. Checkout which events your friend is attending'', ''{
  "audience" : {
      "device_token" : [deviceTokens]
  },
  "notification" : {
       "alert" : "[alert]",
       "ios" : {
         "extra" : { "friendName" : "[friendName]", "notificationType": "[notificationType]"}
      }
  },
  "device_types" : ["ios"]
}'')'

EXEC sp_executesql N'INSERT INTO [MessageTemplate](Id, MessageTemplateType, AlertText, MessageBody) VALUES(2, ''FB_FRIEND_BOOKED_TICKET'', ''Hey, your friend [friendName] just booked [eventName]. Join [friendName] for the event!'', ''{
  "audience" : {
      "device_token" : [deviceTokens]
  },
  "notification" : {
       "alert" : "[alert]",
       "ios" : {
         "extra" : { "friendName" : "[friendName]", "eventId" : "[eventId]", "notificationType": "[notificationType]"}
      }
  },
  "device_types" : ["ios"]
}'')'	

EXEC sp_executesql N'INSERT INTO [MessageTemplate](Id, MessageTemplateType, AlertText, MessageBody) VALUES(3, ''FAV_CHEF_NEW_EVENT_NOTIFICATION'', ''Hey, your favourite [supperClubName] is back with [eventName] on [eventDate]. Come, join the fun!'', ''{
  "audience" : {
      "device_token" : [deviceTokens]
  },
  "notification" : {
       "alert" : "[alert]",
       "ios" : {
         "extra" : { "eventId" : "[eventId]", "notificationType": "[notificationType]"}
      }
  },
  "device_types" : ["ios"]
}'')'	

EXEC sp_executesql N'INSERT INTO [MessageTemplate](Id, MessageTemplateType, AlertText, MessageBody) VALUES(4, ''FAV_EVENT_BOOKING_REMINDER'', ''Hey, your favourite event [eventName] has very few tickets left. Book the tickets before they are gone!!'', ''{
  "audience" : {
      "device_token" : [deviceTokens]
  },
  "notification" : {
       "alert" : "[alert]",
       "ios" : {
         "extra" : { "eventId" : "[eventId]", "notificationType": "[notificationType]"}
      }
  },
  "device_types" : ["ios"]
}'')'		
EXEC sp_executesql N'INSERT INTO [MessageTemplate](Id, MessageTemplateType, AlertText, MessageBody) VALUES(5, ''WAITLIST_EVENT_TICKETS_AVAILABLE'', ''Good news! [eventName] has got some seats now. Book the tickets before they are gone!!'', ''{
  "audience" : {
      "device_token" : [deviceTokens]
  },
  "notification" : {
       "alert" : "[alert]",
       "ios" : {
         "extra" : { "eventId" : "[eventId]", "notificationType": "[notificationType]"}
      }
  },
  "device_types" : ["ios"]
}'')'		
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


