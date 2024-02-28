-- Author:	Swati Agrawal
-- Summary:	This inserts the Email Template for sending the acknowledgement to host
DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.24.Insert_EmailTemplate17'

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
INSERT INTO [dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (17, 'Grub Club Creation Acknowledgement', 'True',
           N'<!DOCTYPE HTML PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Grub Club Email</title>
</head>
<body leftmargin="0" marginwidth="0" topmargin="0" marginheight="0" offset="0" style="color: #202020;font-family: Arial;width: 100%;">

    <center>
        <table border="0" cellpadding="0" cellspacing="0" height="100%" width="100%" id="backgroundTable">
            <tr>
                <td align="center" valign="top">
                    <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateContainer" style="background-color: #FFFFFF;">
                        <tr>
                            <td align="center" valign="top">
                                <span align="center" style="font-size: 1.0em;font-family: Arial;"> 
                                    DO NOT REPLY TO THIS EMAIL</span><br /><br />
                                <!-- // Begin Template Header \ -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateHeader">
                                    <tr>
                                        <td class="headerContent">
                                            <!-- // Header Image \ -->
                                            <img src="[serverURL]Content/images/email/0.Generic_FINAL.jpg"
                                            style="max-width:600px;" id="headerImage campaign-icon;" />
                                        </td>
                                    </tr>
                                </table>
                                <!-- // End Template Header \ -->
                            </td>
                        </tr>
                        <tr>
                            <td align="center" valign="top">
                                <!-- // Begin Template Body \ -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateBody">
                                    <tr>
                                        <td height="316" valign="top" class="bodyContent" style="background-color: #FFFFFF;">
                                            <!-- // Begin Module: Standard Content \ -->
                                            <table border="0" cellpadding="20" cellspacing="0" width="100%">
                                                <tr>
                                                    <td valign="top">
                                                        <div style="color: #808080;font-family: Arial;font-size: 12px;line-height: 150%;text-align: left;">
                                                            <h1 class="h1" style="color: #202020;display: block;font-family: Arial;font-size: 30px;font-weight: bold;line-height: 100%;margin-top: 0;margin-right: 0;margin-bottom: 10px;margin-left: 0;text-align: left;">
                                                                Grubclub Creation Confirmation
                                                            </h1>
                                                            <h2 class="h2" style="color: #808080;display: block;font-family: Arial;font-size: 20px;font-weight: bold;line-height: 100%;margin-top: 0;margin-right: 0;margin-bottom: 10px;margin-left: 0;text-align: left;">                 
                                                                Thank you for taking the time to create your Grub Club.
                                                            </h2>
                                                            <br />
                                                            <p>
                                                            <span style="font-size: 24px; color: #ff7500; font-family: ''Lobster 1.3'', serif">Next steps:</span>
                                                                <ul style="list-style-type:circle">
                                                                    <li>We will review your submission asap here at Grub Club HQ and let you know the outcome</li>
                                                                    <li>We will call you if we have any further questions</li>
                                                                    <li>
                                                                        If approved, we will come back to you with your newly created event URL/booking link for you
                                                                        to send to your users and start marketing
                                                                    </li>
                                                                    <li>
                                                                        This is your Profile URL, to which all approved events will be added: <a href="[grubClubURL]" target="_blank">[grubClubName]</a>
																		
                                                                        <ul style="list-style-type:disc; list-style-position: inside">
                                                                            <li>You will also find your Grub Club (event) URL(s) under: My Events</li>
                                                                        </ul>
                                                                    </li>
                                                                    <li>You can see all Grub Club profile details under: My Account > My Grub Club details</li>
                                                                    <li>
                                                                        <a href="http://grubclub.com/host/tips" target="_blank">Here are some tips on managing the site (content)</a>, just so you know
                                                                        how it works, in case you get approved.
                                                                    </li>
                                                                </ul>
                                                            </p>                                                                                                                                                               <br/>          
                                                            <p>
                                                                Thanks,
                                                                <br />
                                                            The Grub Club Team
                                                            </p>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <!-- // End Template Body \ -->
                            </td>
                        </tr>
                        <tr>
                            <td align="center" valign="top">
                                <!-- // Begin Template Footer \ -->
                                <table border="0" cellpadding="10" cellspacing="0" width="600" id="templateFooter" style="background-color: #4d4a41;border-top: 0px solid #564a40;">
                                    <tr>
                                        <td valign="top" class="footerContent">
                                            <table border="0" cellpadding="10" cellspacing="0" width="100%">
                                                <tr>
                                                    <td valign="top" width="411">
                                                        <div mc:edit="std_footer" style="color: #def4f1;font-family: Arial;font-size: 12px;line-height: 125%;text-align: left;">
                                                            <em>Copyright &copy; 2014 GrubClub, All rights reserved.</em>
                                                            <br />                
                                                            <strong>Our mailing address is:</strong>
                                                            <br />                
                                                            <a href="mailto:[contactEmail]" target="_blank" style="text-decoration: none;color: #ff7500;">eat@grubclub.com</a>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <br />
                </td>
            </tr>
        </table>
    </center>
</body>
</html>' )

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

