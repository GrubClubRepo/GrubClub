-- Author:	Swati Agrawal
-- Summary:	This updates the Email Template for guest booking confirmation email

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.01.UpdateEmailTemplate12'

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

--BookingConfirmedGuest = 4,

DELETE FROM [SupperClub].[dbo].[EmailTemplate]
WHERE [Id] = 12

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (12, 'GrubClub Event Booking Confirmation', 'True',
           N'<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xmlns="http://www.w3.org/1999/xhtml">
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width" />
<!-- For testing only -->
  </head>
  <body style="width: 100% !important; min-width: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; border-top-style: solid; border-top-color: #240003; border-top-width: 10px; background: #4B2409; margin: 0; padding: 0;" bgcolor="#4B2409"><style type="text/css">
a:hover {
color: #2795b6 !important;
}
a:active {
color: #2795b6 !important;
}
a:visited {
color: #2ba6cb !important;
}
h1 a:active {
color: #2ba6cb !important;
}
h2 a:active {
color: #2ba6cb !important;
}
h3 a:active {
color: #2ba6cb !important;
}
h4 a:active {
color: #2ba6cb !important;
}
h5 a:active {
color: #2ba6cb !important;
}
h6 a:active {
color: #2ba6cb !important;
}
h1 a:visited {
color: #2ba6cb !important;
}
h2 a:visited {
color: #2ba6cb !important;
}
h3 a:visited {
color: #2ba6cb !important;
}
h4 a:visited {
color: #2ba6cb !important;
}
h5 a:visited {
color: #2ba6cb !important;
}
h6 a:visited {
color: #2ba6cb !important;
}
table.button:hover td {
background: #2795b6 !important;
}
table.button:visited td {
background: #2795b6 !important;
}
table.button:active td {
background: #2795b6 !important;
}
table.button:hover td a {
color: #fff !important;
}
table.button:visited td a {
color: #fff !important;
}
table.button:active td a {
color: #fff !important;
}
table.button:hover td {
background: #2795b6 !important;
}
table.tiny-button:hover td {
background: #2795b6 !important;
}
table.small-button:hover td {
background: #2795b6 !important;
}
table.medium-button:hover td {
background: #2795b6 !important;
}
table.large-button:hover td {
background: #2795b6 !important;
}
table.button:hover td a {
color: #ffffff !important;
}
table.button:active td a {
color: #ffffff !important;
}
table.button td a:visited {
color: #ffffff !important;
}
table.tiny-button:hover td a {
color: #ffffff !important;
}
table.tiny-button:active td a {
color: #ffffff !important;
}
table.tiny-button td a:visited {
color: #ffffff !important;
}
table.small-button:hover td a {
color: #ffffff !important;
}
table.small-button:active td a {
color: #ffffff !important;
}
table.small-button td a:visited {
color: #ffffff !important;
}
table.medium-button:hover td a {
color: #ffffff !important;
}
table.medium-button:active td a {
color: #ffffff !important;
}
table.medium-button td a:visited {
color: #ffffff !important;
}
table.large-button:hover td a {
color: #ffffff !important;
}
table.large-button:active td a {
color: #ffffff !important;
}
table.large-button td a:visited {
color: #ffffff !important;
}
table.secondary:hover td {
background: #d0d0d0 !important; color: #555;
}
table.secondary:hover td a {
color: #555 !important;
}
table.secondary td a:visited {
color: #555 !important;
}
table.secondary:active td a {
color: #555 !important;
}
table.success:hover td {
background: #457a1a !important;
}
table.alert:hover td {
background: #970b0e !important;
}
a:hover {
color: #ff5200 !important;
}
a:active {
color: #ff5200 !important;
}
a:visited {
color: #ff5200 !important;
}
h1 a:active {
color: #ff5200 !important;
}
h2 a:active {
color: #ff5200 !important;
}
h3 a:active {
color: #ff5200 !important;
}
h4 a:active {
color: #ff5200 !important;
}
h5 a:active {
color: #ff5200 !important;
}
h6 a:active {
color: #ff5200 !important;
}
h1 a:visited {
color: #ff5200 !important;
}
h2 a:visited {
color: #ff5200 !important;
}
h3 a:visited {
color: #ff5200 !important;
}
h4 a:visited {
color: #ff5200 !important;
}
h5 a:visited {
color: #ff5200 !important;
}
h6 a:visited {
color: #ff5200 !important;
}
#fb:hover {
background: #2D4D85 !important; border: 1px solid #2D4D85;
}
table.alert:hover td {
background: #ff5200 !important;
}
@media only screen and (max-width: 600px) {
  table[class="body"] img {
    width: auto !important; height: auto !important;
  }
  table[class="body"] center {
    min-width: 0 !important;
  }
  table[class="body"] .container {
    width: 95% !important;
  }
  table[class="body"] .row {
    width: 100% !important; display: block !important;
  }
  table[class="body"] .wrapper {
    display: block !important; padding-right: 0 !important;
  }
  table[class="body"] .columns {
    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;
  }
  table[class="body"] .column {
    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;
  }
  table[class="body"] .wrapper.first .columns {
    display: table !important;
  }
  table[class="body"] .wrapper.first .column {
    display: table !important;
  }
  table[class="body"] table.columns td {
    width: 100% !important;
  }
  table[class="body"] table.column td {
    width: 100% !important;
  }
  table[class="body"] .columns td.one {
    width: 8.333333% !important;
  }
  table[class="body"] .column td.one {
    width: 8.333333% !important;
  }
  table[class="body"] .columns td.two {
    width: 16.666666% !important;
  }
  table[class="body"] .column td.two {
    width: 16.666666% !important;
  }
  table[class="body"] .columns td.three {
    width: 25% !important;
  }
  table[class="body"] .column td.three {
    width: 25% !important;
  }
  table[class="body"] .columns td.four {
    width: 33.333333% !important;
  }
  table[class="body"] .column td.four {
    width: 33.333333% !important;
  }
  table[class="body"] .columns td.five {
    width: 41.666666% !important;
  }
  table[class="body"] .column td.five {
    width: 41.666666% !important;
  }
  table[class="body"] .columns td.six {
    width: 50% !important;
  }
  table[class="body"] .column td.six {
    width: 50% !important;
  }
  table[class="body"] .columns td.seven {
    width: 58.333333% !important;
  }
  table[class="body"] .column td.seven {
    width: 58.333333% !important;
  }
  table[class="body"] .columns td.eight {
    width: 66.666666% !important;
  }
  table[class="body"] .column td.eight {
    width: 66.666666% !important;
  }
  table[class="body"] .columns td.nine {
    width: 75% !important;
  }
  table[class="body"] .column td.nine {
    width: 75% !important;
  }
  table[class="body"] .columns td.ten {
    width: 83.333333% !important;
  }
  table[class="body"] .column td.ten {
    width: 83.333333% !important;
  }
  table[class="body"] .columns td.eleven {
    width: 91.666666% !important;
  }
  table[class="body"] .column td.eleven {
    width: 91.666666% !important;
  }
  table[class="body"] .columns td.twelve {
    width: 100% !important;
  }
  table[class="body"] .column td.twelve {
    width: 100% !important;
  }
  table[class="body"] td.offset-by-one {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-two {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-three {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-four {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-five {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-six {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-seven {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-eight {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-nine {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-ten {
    padding-left: 0 !important;
  }
  table[class="body"] td.offset-by-eleven {
    padding-left: 0 !important;
  }
  table[class="body"] table.columns td.expander {
    width: 1px !important;
  }
  table[class="body"] .right-text-pad {
    padding-left: 10px !important;
  }
  table[class="body"] .text-pad-right {
    padding-left: 10px !important;
  }
  table[class="body"] .left-text-pad {
    padding-right: 10px !important;
  }
  table[class="body"] .text-pad-left {
    padding-right: 10px !important;
  }
  table[class="body"] .hide-for-small {
    display: none !important;
  }
  table[class="body"] .show-for-desktop {
    display: none !important;
  }
  table[class="body"] .show-for-small {
    display: inherit !important;
  }
  table[class="body"] .hide-for-desktop {
    display: inherit !important;
  }
}
</style>
  <table class="body" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; height: 100%; width: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; background: #4B2409; margin: 0; padding: 0;" bgcolor="#4B2409"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="center" align="center" valign="top" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;">
        <center style="width: 100%; min-width: 580px;">
          <!-- Email Content -->
          <table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
                <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                      <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="left" valign="top">
                            <center style="width: 100%; min-width: 580px;"><p style="color: #fff; text-align: center; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 5px; padding: 0;" align="center">DO NOT REPLY TO THIS EMAIL ADDRESS</p></center>
                          </td>
                        </tr></table></td>
                  </tr></table></td>
            </tr></table><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
                <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; background: #fff; padding: 0px;" bgcolor="#fff"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px;" align="left" valign="top">

                      <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="center" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="center" valign="top">
                           <center style="width: 100%; min-width: 580px;"> <img src="http://grubclub.com/content/images/email/grubclub-logo-png.png" alt="" class="center" width="250" height="70" style="outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; float: none; clear: both; display: block; margin: 0 auto;" align="none" /></center>
                         </td>
                         <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                       </tr></table></td>
                 </tr></table></td>
           </tr></table><!-- END HEADER--><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
              <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px;" align="left" valign="top">
                    <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="center" align="center" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; background: #fff; margin: 0; padding: 0px 0px 10px;" bgcolor="#fff" valign="top">
                          <h2 style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 1.3; word-break: normal; font-size: 36px; margin: 0; padding: 0;" align="center">Your event has been booked!</h2>
                          <p style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0;" align="center">Get ready for a Social and Culinary Experience.</p>

                        </td>
                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                      </tr></table></td>
                </tr></table></td>
          </tr></table><!-- END CONFIRMED MESSAGE --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
              <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; background: #FEFCCC; padding: 0;" align="left" bgcolor="#FEFCCC"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                    <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="six sub columns left-text-pad" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top">
                          <h4 style="color: #ff7400; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 28px; margin: 0; padding: 0;" align="left">[eventName]</h4>
                          <h5 style="color: #4B2409; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0;" align="left">by: [grubClubName]</h5>                  
                        </td>                       
                        <td class="six sub columns last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="left" valign="top">
                          <p style="text-align: right; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0 10px 0 0;" align="right">
                            [eventDate]
                            <br /><a href="http://www.google.com/calendar/event?action=TEMPLATE&text=[eventName]&dates=[calStartDateTime]/[calEndDateTime]&details=For+details,+please+visit+[eventUrl]&location=[eventAddress],[eventAddress2],[eventCity] [eventPostCode]&trp=false&sprop=&sprop=name:" target="_blank" rel="nofollow" style="color: #ff7400; text-decoration: none;">Add to my calendar</a>                            
                          </p>
                          <p style="text-align: right; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0 10px 0 0;" align="right">
                            [eventAddress] <br />
                            [eventAddress2] <br />
                            [eventCity] <br />
                            [eventPostCode] <br /><a href="http://maps.google.co.uk/?q=[eventAddress],[eventAddress2],[eventCity] [eventPostCode]" style="color: #ff7400; text-decoration: none;">View on map </a>     </p>
                          </td>
                          <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                        </tr></table></td>
                  </tr></table></td>
            </tr></table><!-- END DETAILS --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
                <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 0px; padding: 0px;" align="left" valign="top">
                      <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="panel" id="details" style="background-color: #FFCE6F !important; word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px; border: 1px none #d9d9d9;" align="left" bgcolor="#FFCE6F !important" valign="top">
                            <p style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0;" align="center">Please be prompt, arriving early or late can affect everyone''s dining experience.</p>
                            <p style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0;" align="center"><b>Special Requirements:</b> [bookingRequirements]</p>
                            <p style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0;" align="center"><b>Alcohol Policy:</b> [alcoholPolicy]</p>
                          </td>
                          <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                        </tr></table></td>
                  </tr></table></td>
            </tr></table><!-- END MISC DETAILS --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
                <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px;" align="left" valign="top">
                      <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="panel" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; background: #FEFCCC; margin: 0; padding: 10px; border: 1px none #d9d9d9;" align="left" bgcolor="#FEFCCC" valign="top">
                            <table class="receipt" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; margin-right: auto; margin-left: auto; width: 100%; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="six sub-columns" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 10px 10px 0px;" align="left" valign="top">
                                  <b>Receipt Date:</b> [receiptDate]
                                </td>
                                <td class="six sub-columns" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-left-style: solid; border-left-width: 1px; border-left-color: #FFCE6F; margin: 0; padding: 0px 10px 10px 5px;" align="left" valign="top">
                                  <b> Booking No.:</b> [bookingReference]
                                </td>
                              </tr><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="six sub-columns" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 10px 10px 0px;" align="left" valign="top">
                                 <b> Diner Name:</b> [userName]
                               </td>
                               <td class="six sub-columns" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-left-style: solid; border-left-width: 1px; border-left-color: #FFCE6F; margin: 0; padding: 0px 10px 10px 5px;" align="left" valign="top">
                                 <b>Event:</b> [eventName]
                               </td>
                             </tr></table></td>
                       </tr></table></td>
                 </tr></table></td>
           </tr></table><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; background: #FEFCCC; margin: 0 auto; padding: 0;" bgcolor="#FEFCCC"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
              <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                    <table class="twelve columns menurow" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="border-bottom-style: solid; border-bottom-color: #FFCE6F; border-bottom-width: 1px; border-top-style: solid; border-top-color: #FFCE6F; border-top-width: 1px; vertical-align: top; text-align: left; padding: 0;" align="left"><td colspan="2" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top"><b>Menu Option</b></td>
                        <td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top"><b>Quantity</b></td>
                        <td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top"><b>Total</b></td>
                      </tr>
                      [menuRows]
                   </table></td>
               </tr></table></td>

         </tr></table><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
              <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                   <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; background: #fff; padding: 0;" align="left" bgcolor="#fff"><td class="text-pad-left center" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 10px;" align="center" valign="top"> 
                       <p style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 0;" align="center"> [comments]</p>
                      </td>
                     </tr></table></td>
               </tr></table></td>
         </tr></table><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
            <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0px 0px;" align="left" valign="top">
                 <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="center" align="center" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; background: #fff; margin: 0; padding: 0px 0px 10px;" bgcolor="#fff" valign="top">
                      <h5 style="text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0;" align="center">
                        Got Questions? Email: 
                        <a href="mailto:[contactEmail]" target="_blank" style="text-decoration: none; color: #ff7500;">[contactEmail]</a>
                      </h5>
                    </td>
                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                  </tr></table></td>
            </tr></table></td>
      </tr></table><!-- END TICKET DETAILS --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">  
          <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 20px 0px 0px;" align="left" valign="top">

                <table class="four columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="left" valign="top">
                      <table class="button" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; overflow: hidden; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td id="fb" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #ffffff; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; display: block; width: auto !important; background-color: #3B5998 !important; margin: 0; padding: 8px 0; border: 1px solid #3b5909;" align="center" bgcolor="#3B5998 !important" valign="top">
                            <!-- example hard coded link; the url needs to be populated on the server side-->
                            <img src="http://grubclub.com/content/images/email/facebook.jpg" alt="" width="20" height="20" style="width: 20px; height: 20px; margin-left: 2px; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; max-width: 100%; float: left; clear: both; display: block;" align="left" /><a href="http://www.facebook.com/sharer/sharer.php?u=[eventUrl]" target="_blank" style="color: #ffffff; text-decoration: none; font-weight: bold; font-family: Helvetica, Arial, sans-serif; font-size: 16px; display: block; height: 100%; width: 100%;">
                              Share This Event
                            </a>
                          </td>
                        </tr></table></td>
                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                  </tr></table></td>
              <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 20px 0px 0px;" align="left" valign="top">
                <table class="four columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="left" valign="top">
                     <table class="button" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; overflow: hidden; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #ffffff; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; display: block; width: auto !important; background: #2ba6cb; margin: 0; padding: 8px 0; border: 1px solid #2284a1;" align="center" bgcolor="#2ba6cb" valign="top">
                          <!-- example hard coded link; the params need to be populated on the server side-->
                          <a href="http://twitter.com/intent/tweet?url=[eventUrl]&text=140charDescriptionOfEvent&via=Grub_Club" target="_blank" style="color: #ffffff; text-decoration: none; font-weight: bold; font-family: Helvetica, Arial, sans-serif; font-size: 16px; display: block; height: 100%; width: 100%;">
                            <img src="http://grubclub.com/content/images/email/twitter.png" alt="" width="20" height="20" style="width: 20px; height: 20px; margin-left: 5px; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; max-width: 100%; float: left; clear: both; display: block; border: none;" align="left" />
                            Tweet This Event</a>                              
                          </td>
                        </tr></table></td>
                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                  </tr></table></td>
              <td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                <table class="four columns last" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px;" align="left" valign="top">
                      <table class="button alert" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; overflow: hidden; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #ffffff; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; display: block; width: auto !important; background: #ff7400; margin: 0; padding: 8px 0; border: 1px solid #ff5200;" align="center" bgcolor="#ff7400" valign="top">
                            <img src="http://grubclub.com/content/images/email/gciconflat.png" alt="" width="20" height="20" style="width: 20px; height: 20px; margin-left: 5px; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; max-width: 100%; float: left; clear: both; display: block;" align="left" /><a href="http://grubclub.com/search/london/pop-up-restaurants" target="_blank" style="color: #ffffff; text-decoration: none; font-weight: bold; font-family: Helvetica, Arial, sans-serif; font-size: 16px; display: block; height: 100%; width: 100%;">Other GrubClubs</a>                               

                          </td>
                        </tr></table></td>
                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                  </tr></table></td>   
            </tr></table></td>
      </tr></table><!-- END BUTTONS --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
          <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; background: #FFCE6F; margin: 0; padding: 10px 0px 0px;" align="left" bgcolor="#FFCE6F" valign="top">

                <center style="width: 100%; min-width: 580px;">

                  <img class="center" src="http://grubclub.com/content/images/email/meetpeople.jpg" alt="" width="580" style="max-width: 580px; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; float: none; clear: both; display: block; margin: 0 auto;" align="none" /></center>

              </td>
            </tr></table></td>
      </tr></table><!-- END IMAGE --><table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
          <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"><tr style="vertical-align: top; text-align: left; padding: 0;" align="left"><td class="center" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 0px;" align="center" valign="top">
                <center style="width: 100%; min-width: 580px;"><p style="text-align: center; color: #fff; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0 0 10px; padding: 10px 0 0;" align="center">Copyright &#169; 2014 GrubClub, All rights reserved.</p></center>
              </td>
            </tr></table></td>
      </tr></table><!-- END COPYRIGHT --><!--  </td>
          </tr>
        </table> --><!-- END CONTAINER --></center>
    </td>
  </tr></table></body>
</html>'
           )

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