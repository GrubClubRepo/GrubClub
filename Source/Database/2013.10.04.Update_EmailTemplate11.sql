-- Author:	Swati
-- Summary:	Updated system Email Templates

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.10.04.Update_EmailTemplate11'

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


DELETE FROM [dbo].[EmailTemplate]
WHERE [Id] = 11

INSERT INTO [dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (11, 'GrubClub Event Booking Confirmation', 'True',
           N'                        <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"> <html>     <head>         <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">    <title>Grub Club Email</title>  </head>  <body leftmargin="0" marginwidth="0" topmargin="0" marginheight="0" offset="0" style="color: #202020;font-family: Arial;width: 100%;">      <center>          <table border="0" cellpadding="0" cellspacing="0" height="100%" width="100%" id="backgroundTable">    <tr>                  <td align="center" valign="top">                       <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateContainer" style="background-color: #FFFFFF;">                          <tr>                              <td align="center" valign="top">  <span align="center" style="font-size: 1.0em;font-family: Arial;"> DO NOT REPLY TO THIS EMAIL</span><br/><br/>                                    <!-- // Begin Template Header \ -->                                  <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateHeader">                                         <tr>                                             <td class="headerContent">                                                                                           <!-- // Header Image \ -->                                              <img src="[serverURL]Content/images/email/3.Your_event_FINAL.jpg" style="max-width:600px;" id="headerImage campaign-icon">                                                                                          </td>                                         </tr>                                     </table>                                     <!-- // End Template Header \ -->                                 </td>                             </tr>                          <tr>                              <td align="center" valign="top">                                     <!-- // Begin Template Body \ -->                                  <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateBody">                                      <tr>                                             <td height="316" valign="top" class="bodyContent" style="background-color: #FFFFFF;">                                                                                  <!-- // Begin Module: Standard Content \ -->             <table border="0" cellpadding="20" cellspacing="0" width="100%">                                                     <tr>                                                         <td valign="top">                                                             <div style="color: #808080;font-family: Arial;font-size: 12px;line-height: 150%;text-align: left;">                 <h1 class="h1" style="color: #202020;display: block;font-family: Arial;font-size: 30px;font-weight: bold;line-height: 100%;margin-top: 0;margin-right: 0;margin-bottom: 10px;margin-left: 0;text-align: left;">                 Your event has been booked!</h1>                 <h2 class="h2" style="color: #808080;display: block;font-family: Arial;font-size: 20px;font-weight: bold;line-height: 100%;margin-top: 0;margin-right: 0;margin-bottom: 10px;margin-left: 0;text-align: left;">                 Get ready for a Social and Culinary Experience.</h2>                 <br>                              <p>                  Here are the details of your event:                 </p>                 <p>                  Grub Club: [grubClubName]<br />                  Event Name: [eventName]<br />                  Event Date: [eventDate]<br />                  Feel free to arrive anytime from [eventStartTime]. The venue closes at [eventEndTime].                 </p>                 <p>                  Special Requirements: [bookingRequirements]                 </p>                 <p>                  Event Address: [eventAddress], [eventAddress2], [eventCity] [eventPostCode]<br />                 </p>                 <p>                 <a href="http://maps.google.co.uk/?q=[eventCity] [eventPostCode]" style="color: #ff7500;text-decoration: none;">Click here for Location Map</a><br />                         </p>               <p>                  Alcohol Policy: [alcoholPolicy]                 </p>                 <p>                  Your Receipt:<br/>                  <table border="1px" cellpadding="2" cellspacing="0" width="100%">                   <tr>                    <td width="25%">                     <strong>Receipt Date:</strong>                     </td>                    <td width="25%">                     [receiptDate]                    </td>                    <td width="25%">                     <strong>Booking Reference:</strong>                    </td>                    <td width="25%">                     [bookingReference]                    </td>                   </tr>                   <tr>                    <td><strong>Diner Name</strong></td>                    <td><strong>Event</strong></td>                    <td><strong>Quantity</strong></td>                    <td><strong>Amount Paid</strong></td>                   </tr>                   <tr>                    <td>[userName]</td>                    <td>[eventName]</td>                    <td>[numberTickets]</td>                    <td>[totalPaid]</td>                   </tr>                  </table>                 </p>        [comments]         <br />                 <p>                  Thanks,                  <br />                  The Grub Club Team                 </p>                </div>               </td>                                                     </tr>             </table>                                                      </td>                                         </tr>                                     </table>                                     <!-- // End Template Body \ -->                                 </td>                             </tr>                          <tr>                              <td align="center" valign="top">                                     <!-- // Begin Template Footer \ -->                                  <table border="0" cellpadding="10" cellspacing="0" width="600" id="templateFooter" style="background-color: #4d4a41;border-top: 0px solid #564a40;">                                      <tr>                                          <td valign="top" class="footerContent">                                                        <table border="0" cellpadding="10" cellspacing="0" width="100%">              <tr>               <td valign="top" width="411">                <div mc:edit="std_footer" style="color: #def4f1;font-family: Arial;font-size: 12px;line-height: 125%;text-align: left;"><em>Copyright &copy; 2012 GrubClub, All rights reserved.</em>                <br>                <strong>Our mailing address is:</strong>                <br>                <a href="mailto:[contactEmail]" target="_blank" style="text-decoration: none;color: #ff7500;">[contactEmail]</a></div>               </td>              </tr>             </table>            </td>                                         </tr>                                     </table>                                                                </td>                             </tr>                         </table>                         <br>                     </td>               </tr>           </table>         </center> </body> </html>' )

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