-- Author:	Swati
-- Summary:	Made the address to be filled in dynamically

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.01.18.UpdateEmailTemplate25'

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
WHERE [Id] = 25

INSERT INTO [dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (25, 'GrubClub - Your wishlisted event is running out of tickets!', 'True',
          N'   <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">   <html xmlns="http://www.w3.org/1999/xhtml">     <head>      <!-- NAME: 1 COLUMN -->            <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">            <meta name="viewport" content="width=device-width, initial-scale=1.0">               <title>Your Wishlisted event is running out of tickets</title>         </head>          <body leftmargin="0" marginwidth="0" topmargin="0" marginheight="0" offset="0">         <center>                   <table align="center" border="0" cellpadding="0" cellspacing="0" height="100%" width="100%" id="bodyTable">                          <tr>                     <td align="center" valign="top" id="bodyCell">                                      <!-- BEGIN TEMPLATE // -->                                        <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateContainer">                                                               <tr>                                                        <td align="center" valign="top">                                <!-- BEGIN PREHEADER // -->                                DO NOT REPLY TO THIS EMAIL                               <table border="0" cellpadding="0" cellspacing="0" width="600" id="templatePreheader">                                         <tr>                                          <td valign="top" class="preheaderContainer" style="padding-top:9px;"><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">     <tbody class="mcnTextBlockOuter">         <tr>             <td valign="top" class="mcnTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="366" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;">                                                      ...There''s Only few Tickets Left! <!---&nbsp;<a href="http://grubclub.com/pop-up-restaurants?utm_source=wishlist_soldout&amp;utm_medium=banner_top" target="_blank">See all Events</a>   -->                      </td>                     </tr>                 </tbody></table>                                  <table align="right" border="0" cellpadding="0" cellspacing="0" width="197" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;">                                                    <!--  <a href="*|ARCHIVE|*" target="_blank">View this email in your browser</a> -->                        </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table></td>                                         </tr>                                     </table>                                     <!-- // END PREHEADER -->                                 </td>                             </tr>                             <tr>                                 <td align="center" valign="top">                                     <!-- BEGIN HEADER // -->                                     <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateHeader">                                         <tr>                                             <td valign="top" class="headerContainer"><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnImageBlock">     <tbody class="mcnImageBlockOuter">             <tr>                 <td valign="top" style="padding:9px" class="mcnImageBlockInner">                     <table align="left" width="100%" border="0" cellpadding="0" cellspacing="0" class="mcnImageContentContainer">                         <tbody><tr>                             <td class="mcnImageContent" valign="top" style="padding-right: 9px; padding-left: 9px; padding-top: 0; padding-bottom: 0;">                                                                      <a href="http://grubclub.com?utm_source=wishlist_soldout&amp;utm_medium=banner_top" title="" class="" target="_blank">                                         <img align="left" alt="" src="https://grubclub.com/content/images/email/5f0504d1-ed29-4cba-83ae-123fb1293e75.png" width="564" style="max-width:1408px; padding-bottom: 0; display: inline !important; vertical-align: bottom;" class="mcnImage">                                     </a>                                                              </td>                         </tr>                     </tbody></table>                 </td>             </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnImageBlock">     <tbody class="mcnImageBlockOuter">             <tr>                 <td valign="top" style="padding:9px" class="mcnImageBlockInner">                     <table align="left" width="100%" border="0" cellpadding="0" cellspacing="0" class="mcnImageContentContainer">                         <tbody><tr>                             <td class="mcnImageContent" valign="top" style="padding-right: 9px; padding-left: 9px; padding-top: 0; padding-bottom: 0;">                                                                                                               <img align="left" alt="Fine Dining Under �30" src="https://grubclub.com/content/images/email/f960e9e4-3305-47cf-a56f-25b72284fb25.jpg" width="564" style="max-width:800px; padding-bottom: 0; display: inline !important; vertical-align: bottom;" class="mcnImage">                                                                                                   </td>                         </tr>                     </tbody></table>                 </td>             </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">     <tbody class="mcnDividerBlockOuter">         <tr>             <td class="mcnDividerBlockInner" style="padding: 18px;">                 <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">                     <tbody><tr>                         <td>                             <span></span>                         </td>                     </tr>                 </tbody></table>             </td>         </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">     <tbody class="mcnTextBlockOuter">         <tr>             <td valign="top" class="mcnTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">                                                      <div><span style="color:#000000"><strong><span style="font-size:24px">Hi&nbsp;[userName]&nbsp;</span></strong></span></div>                          </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">     <tbody class="mcnTextBlockOuter">         <tr>             <td valign="top" class="mcnTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">                                                      <div><span style="font-size:18px"><strong>The dining experience you''ve wishlisted <span style="color:#FF8C00">has nearly sold out!</span></strong></span></div>                          </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">     <tbody class="mcnDividerBlockOuter">         <tr>             <td class="mcnDividerBlockInner" style="padding: 18px;">                 <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">                     <tbody><tr>                         <td>                             <span></span>                         </td>                     </tr>                 </tbody></table>             </td>         </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnBoxedTextBlock">     <tbody class="mcnBoxedTextBlockOuter">         <tr>             <td valign="top" class="mcnBoxedTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="366" class="mcnBoxedTextContentContainer">                     <tbody><tr>                                                  <td class="mcnBoxedTextContentColumn" style="padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;">                                                      <table border="0" cellpadding="18" cellspacing="0" class="mcnTextContentContainer" width="100%" style="border: 1px solid #FFFFFF;background-color: #FFFFFF;">                                 <tbody><tr>                                     <td valign="top" class="mcnTextContent">                                         <a href="[eventUrl]">[eventName]</a>,<br> [eventDateTime],<br> [eventCity], [eventPostCode]<br> &pound;[eventCost]                                     </td>                                 </tr>                             </tbody></table>                         </td>                     </tr>                 </tbody></table>                                  <table align="right" border="0" cellpadding="0" cellspacing="0" width="197" class="mcnBoxedTextContentContainer">                     <tbody><tr>                                                  <td class="mcnBoxedTextContentColumn" style="padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;">                                                      <table border="0" cellpadding="18" cellspacing="0" class="mcnTextContentContainer" width="100%" style="border: 1px solid #FFFFFF;background-color: #FFFFFF;">                                 <tbody><tr>                                     <td valign="top" class="mcnTextContent">                                         <div style="text-align: left;"><a href="[bookingUrl]" style="color:#FF8C00;" target="_blank"><span style="color:#FF8C00;font-size:28px;font-weight: bold;">Book Now!</span></a></div>                                      </td>                                 </tr>                             </tbody></table>                         </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">     <tbody class="mcnTextBlockOuter">         <tr>             <td valign="top" class="mcnTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">                                                      <div>We thought we''d just let you know that the dining experience you''ve wishlisted has <strong>only few tickets left!</strong><br> &nbsp;<br> We''d hate for you to miss out so click the <span style="color:#FF8C00"><strong>Book Now</strong></span> button above to secure your seats at the table!<br> <br> <strong>Have a lovely day,</strong><br> Team Grub Club<br> &nbsp;</div>                          </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table></td>                                         </tr>                                     </table>                                     <!-- // END HEADER -->                                 </td>                             </tr>                             <tr>                                 <td align="center" valign="top">                                     <!-- BEGIN BODY // -->                                     <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateBody">                                         <tr>                                             <td valign="top" class="bodyContainer"></td>                                         </tr>                                     </table>                                     <!-- // END BODY -->                                 </td>                             </tr>                             <tr>                                 <td align="center" valign="top">                                     <!-- BEGIN FOOTER // -->                                     <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateFooter">                                         <tr>                                             <td valign="top" class="footerContainer" style="padding-bottom:9px;"><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">     <tbody class="mcnTextBlockOuter">         <tr>             <td valign="top" class="mcnTextBlockInner">                                  <table align="left" border="0" cellpadding="0" cellspacing="0" width="282" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;">                                                      <br> Copyright &copy; 2015 GrubClub, All rights reserved.&nbsp;<br> Our mailing address is:&nbsp;<br> <a href="mailto:eat@grubclub.com" target="_blank">[contactEmail]</a>                         </td>                     </tr>                 </tbody></table>                                  <table align="right" border="0" cellpadding="0" cellspacing="0" width="282" class="mcnTextContentContainer">                     <tbody><tr>                                                  <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;">                                                      <div style="text-align: right;"><br> <a href="http://www.grubclub.com">www.grubclub.com</a><br> [officeAddress]</div>                          </td>                     </tr>                 </tbody></table>                              </td>         </tr>     </tbody> </table></td>                                         </tr>                                     </table>                                     <!-- // END FOOTER -->                                 </td>                             </tr>                         </table>                         <!-- // END TEMPLATE -->                     </td>                 </tr>             </table>         </center>     </body> </html>           '      )

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