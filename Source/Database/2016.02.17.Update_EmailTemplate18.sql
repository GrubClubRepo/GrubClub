-- Author:	Swati
-- Summary:	Updated links with new URLs

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.17.Update_EmailTemplate18'

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

--Grub Club Approval Confirmation! = 18,

DELETE FROM [dbo].[EmailTemplate]
WHERE [Id] = 18

INSERT INTO [dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (18, 'Grub Club Approval Confirmation', 'True',
        N'

<!DOCTYPE HTML PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <!-- NAME: 1 COLUMN -->
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Grub Club Email</title>

    <style type="text/css">
                body, #bodyTable, #bodyCell {
                    height: 100% !important;
                    margin: 0;
                    padding: 0;
                    width: 100% !important;
                }

                table {
                    border-collapse: collapse;
                }

                img, a img {
                    border: 0;
                    outline: none;
                    text-decoration: none;
                }

                h1, h2, h3, h4, h5, h6 {
                    margin: 0;
                    padding: 0;
                }

                p {
                    margin: 1em 0;
                    padding: 0;
                }

                a {
                    word-wrap: break-word;
                }

                .ReadMsgBody {
                    width: 100%;
                }

                .ExternalClass {
                    width: 100%;
                }

                    .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {
                        line-height: 100%;
                    }

                table, td {
                    mso-table-lspace: 0pt;
                    mso-table-rspace: 0pt;
                }

                #outlook a {
                    padding: 0;
                }

                img {
                    -ms-interpolation-mode: bicubic;
                }

                body, table, td, p, a, li, blockquote {
                    -ms-text-size-adjust: 100%;
                    -webkit-text-size-adjust: 100%;
                }

                #bodyCell {
                    padding: 20px;
                }

                .mcnImage {
                    vertical-align: bottom;
                }

                .mcnTextContent img {
                    height: auto !important;
                }
                /*
            @tab Page
            @section background style
            @tip Set the background color and top border for your email. You may want to choose colors that match your company''s branding.
            */
                body, #bodyTable {
                    /*@editable*/ background-color: #eae6dd;
                }
                /*
            @tab Page
            @section background style
            @tip Set the background color and top border for your email. You may want to choose colors that match your company''s branding.
            */
                #bodyCell {
                    /*@editable*/ border-top: 0;
                }
                /*
            @tab Page
            @section email border
            @tip Set the border for your email.
            */
                #templateContainer {
                    /*@editable*/ border: 0;
                }
                /*
            @tab Page
            @section heading 1
            @tip Set the styling for all first-level headings in your emails. These should be the largest of your headings.
            @style heading 1
            */
                h1 {
                    /*@editable*/ color: #606060 !important;
                    display: block;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 40px;
                    /*@editable*/ font-style: normal;
                    /*@editable*/ font-weight: bold;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ letter-spacing: -1px;
                    margin: 0;
                    /*@editable*/ text-align: left;
                }
                /*
            @tab Page
            @section heading 2
            @tip Set the styling for all second-level headings in your emails.
            @style heading 2
            */
                h2 {
                    /*@editable*/ color: #404040 !important;
                    display: block;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 26px;
                    /*@editable*/ font-style: normal;
                    /*@editable*/ font-weight: bold;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ letter-spacing: -.75px;
                    margin: 0;
                    /*@editable*/ text-align: left;
                }
                /*
            @tab Page
            @section heading 3
            @tip Set the styling for all third-level headings in your emails.
            @style heading 3
            */
                h3 {
                    /*@editable*/ color: #606060 !important;
                    display: block;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 18px;
                    /*@editable*/ font-style: normal;
                    /*@editable*/ font-weight: bold;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ letter-spacing: -.5px;
                    margin: 0;
                    /*@editable*/ text-align: left;
                }
                /*
            @tab Page
            @section heading 4
            @tip Set the styling for all fourth-level headings in your emails. These should be the smallest of your headings.
            @style heading 4
            */
                h4 {
                    /*@editable*/ color: #808080 !important;
                    display: block;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 16px;
                    /*@editable*/ font-style: normal;
                    /*@editable*/ font-weight: bold;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ letter-spacing: normal;
                    margin: 0;
                    /*@editable*/ text-align: left;
                }
                /*
            @tab Preheader
            @section preheader style
            @tip Set the background color and borders for your email''s preheader area.
            */
                #templatePreheader {
                    /*@editable*/ background-color: #eae6dd;
                    /*@editable*/ border-top: 0;
                    /*@editable*/ border-bottom: 0;
                }
                /*
            @tab Preheader
            @section preheader text
            @tip Set the styling for your email''s preheader text. Choose a size and color that is easy to read.
            */
                .preheaderContainer .mcnTextContent, .preheaderContainer .mcnTextContent p {
                    /*@editable*/ color: #606060;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 11px;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ text-align: left;
                }
                    /*
            @tab Preheader
            @section preheader link
            @tip Set the styling for your email''s header links. Choose a color that helps them stand out from your text.
            */
                    .preheaderContainer .mcnTextContent a {
                        /*@editable*/ color: #606060;
                        /*@editable*/ font-weight: normal;
                        /*@editable*/ text-decoration: underline;
                    }
                /*
            @tab Header
            @section header style
            @tip Set the background color and borders for your email''s header area.
            */
                #templateHeader {
                    /*@editable*/ background-color: #FFFFFF;
                    /*@editable*/ border-top: 0;
                    /*@editable*/ border-bottom: 0;
                }
                /*
            @tab Header
            @section header text
            @tip Set the styling for your email''s header text. Choose a size and color that is easy to read.
            */
                .headerContainer .mcnTextContent, .headerContainer .mcnTextContent p {
                    /*@editable*/ color: #606060;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 15px;
                    /*@editable*/ line-height: 150%;
                    /*@editable*/ text-align: left;
                }
                    /*
            @tab Header
            @section header link
            @tip Set the styling for your email''s header links. Choose a color that helps them stand out from your text.
            */
                    .headerContainer .mcnTextContent a {
                        /*@editable*/ color: #6DC6DD;
                        /*@editable*/ font-weight: normal;
                        /*@editable*/ text-decoration: underline;
                    }
                /*
            @tab Body
            @section body style
            @tip Set the background color and borders for your email''s body area.
            */
                #templateBody {
                    /*@editable*/ background-color: #FFFFFF;
                    /*@editable*/ border-top: 0;
                    /*@editable*/ border-bottom: 0;
                }
                /*
            @tab Body
            @section body text
            @tip Set the styling for your email''s body text. Choose a size and color that is easy to read.
            */
                .bodyContainer .mcnTextContent, .bodyContainer .mcnTextContent p {
                    /*@editable*/ color: #606060;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 15px;
                    /*@editable*/ line-height: 150%;
                    /*@editable*/ text-align: left;
                }
                    /*
            @tab Body
            @section body link
            @tip Set the styling for your email''s body links. Choose a color that helps them stand out from your text.
            */
                    .bodyContainer .mcnTextContent a {
                        /*@editable*/ color: #f5b40f;
                        /*@editable*/ font-weight: normal;
                        /*@editable*/ text-decoration: underline;
                    }
                /*
            @tab Footer
            @section footer style
            @tip Set the background color and borders for your email''s footer area.
            */
                #templateFooter {
                    /*@editable*/ background-color: #eae6dd;
                    /*@editable*/ border-top: 0;
                    /*@editable*/ border-bottom: 0;
                }
                /*
            @tab Footer
            @section footer text
            @tip Set the styling for your email''s footer text. Choose a size and color that is easy to read.
            */
                .footerContainer .mcnTextContent, .footerContainer .mcnTextContent p {
                    /*@editable*/ color: #606060;
                    /*@editable*/ font-family: Helvetica;
                    /*@editable*/ font-size: 11px;
                    /*@editable*/ line-height: 125%;
                    /*@editable*/ text-align: left;
                }
                    /*
            @tab Footer
            @section footer link
            @tip Set the styling for your email''s footer links. Choose a color that helps them stand out from your text.
            */
                    .footerContainer .mcnTextContent a {
                        /*@editable*/ color: #606060;
                        /*@editable*/ font-weight: normal;
                        /*@editable*/ text-decoration: underline;
                    }

                @media only screen and (max-width: 480px) {
                    body, table, td, p, a, li, blockquote {
                        -webkit-text-size-adjust: none !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    body {
                        width: 100% !important;
                        min-width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[id=bodyCell] {
                        padding: 10px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnTextContentContainer] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnBoxedTextContentContainer] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcpreview-image-uploader] {
                        width: 100% !important;
                        display: none !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    img[class=mcnImage] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnImageGroupContentContainer] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageGroupContent] {
                        padding: 9px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageGroupBlockInner] {
                        padding-bottom: 0 !important;
                        padding-top: 0 !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    tbody[class=mcnImageGroupBlockOuter] {
                        padding-bottom: 9px !important;
                        padding-top: 9px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnCaptionTopContent], table[class=mcnCaptionBottomContent] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnCaptionLeftTextContentContainer], table[class=mcnCaptionRightTextContentContainer], table[class=mcnCaptionLeftImageContentContainer], table[class=mcnCaptionRightImageContentContainer], table[class=mcnImageCardLeftTextContentContainer], table[class=mcnImageCardRightTextContentContainer] {
                        width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardLeftImageContent], td[class=mcnImageCardRightImageContent] {
                        padding-right: 18px !important;
                        padding-left: 18px !important;
                        padding-bottom: 0 !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardBottomImageContent] {
                        padding-bottom: 9px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardTopImageContent] {
                        padding-top: 18px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardLeftImageContent], td[class=mcnImageCardRightImageContent] {
                        padding-right: 18px !important;
                        padding-left: 18px !important;
                        padding-bottom: 0 !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardBottomImageContent] {
                        padding-bottom: 9px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnImageCardTopImageContent] {
                        padding-top: 18px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    table[class=mcnCaptionLeftContentOuter] td[class=mcnTextContent], table[class=mcnCaptionRightContentOuter] td[class=mcnTextContent] {
                        padding-top: 9px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnCaptionBlockInner] table[class=mcnCaptionTopContent]:last-child td[class=mcnTextContent] {
                        padding-top: 18px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnBoxedTextContentColumn] {
                        padding-left: 18px !important;
                        padding-right: 18px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=mcnTextContent] {
                        padding-right: 18px !important;
                        padding-left: 18px !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section template width
            @tip Make the template fluid for portrait or landscape view adaptability. If a fluid layout doesn''t work for you, set the width to 300px instead.
            */
                    table[id=templateContainer], table[id=templatePreheader], table[id=templateHeader], table[id=templateBody], table[id=templateFooter] {
                        /*@tab Mobile Styles
        @section template width
        @tip Make the template fluid for portrait or landscape view adaptability. If a fluid layout doesn''t work for you, set the width to 300px instead.*/ max-width: 600px !important;
                        /*@editable*/ width: 100% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section heading 1
            @tip Make the first-level headings larger in size for better readability on small screens.
            */
                    h1 {
                        /*@editable*/ font-size: 24px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section heading 2
            @tip Make the second-level headings larger in size for better readability on small screens.
            */
                    h2 {
                        /*@editable*/ font-size: 20px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section heading 3
            @tip Make the third-level headings larger in size for better readability on small screens.
            */
                    h3 {
                        /*@editable*/ font-size: 18px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section heading 4
            @tip Make the fourth-level headings larger in size for better readability on small screens.
            */
                    h4 {
                        /*@editable*/ font-size: 16px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section Boxed Text
            @tip Make the boxed text larger in size for better readability on small screens. We recommend a font size of at least 16px.
            */
                    table[class=mcnBoxedTextContentContainer] td[class=mcnTextContent], td[class=mcnBoxedTextContentContainer] td[class=mcnTextContent] p {
                        /*@editable*/ font-size: 18px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section Preheader Visibility
            @tip Set the visibility of the email''s preheader on small screens. You can hide it to save space.
            */
                    table[id=templatePreheader] {
                        /*@editable*/ display: block !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section Preheader Text
            @tip Make the preheader text larger in size for better readability on small screens.
            */
                    td[class=preheaderContainer] td[class=mcnTextContent], td[class=preheaderContainer] td[class=mcnTextContent] p {
                        /*@editable*/ font-size: 14px !important;
                        /*@editable*/ line-height: 115% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section Header Text
            @tip Make the header text larger in size for better readability on small screens.
            */
                    td[class=headerContainer] td[class=mcnTextContent], td[class=headerContainer] td[class=mcnTextContent] p {
                        /*@editable*/ font-size: 18px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section Body Text
            @tip Make the body text larger in size for better readability on small screens. We recommend a font size of at least 16px.
            */
                    td[class=bodyContainer] td[class=mcnTextContent], td[class=bodyContainer] td[class=mcnTextContent] p {
                        /*@editable*/ font-size: 18px !important;
                        /*@editable*/ line-height: 125% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    /*
            @tab Mobile Styles
            @section footer text
            @tip Make the body content text larger in size for better readability on small screens.
            */
                    td[class=footerContainer] td[class=mcnTextContent], td[class=footerContainer] td[class=mcnTextContent] p {
                        /*@editable*/ font-size: 14px !important;
                        /*@editable*/ line-height: 115% !important;
                    }
                }

                @media only screen and (max-width: 480px) {
                    td[class=footerContainer] a[class=utilityLink] {
                        display: block !important;
                    }
                }
    </style>
</head>
<body leftmargin="0" marginwidth="0" topmargin="0" marginheight="0" offset="0">
    <center>
        <table align="center" border="0" cellpadding="0" cellspacing="0" height="100%" width="100%" id="bodyTable">
            <tr>
                <td align="center" valign="top" id="bodyCell">
                    <!-- BEGIN TEMPLATE // -->
                    <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateContainer">
                        <tr>
                            <td align="center" valign="top">
                                <!-- BEGIN PREHEADER // -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templatePreheader">
                                    <tr>
                                        <td valign="top" class="mcnTextBlockInner">
                                            <table align="center" border="0" cellpadding="0" cellspacing="0" width="300" class="mcnTextContentContainer">
                                                <tbody>
                                                    <tr>

                                                        <td width="100%" valign="top" class="mcnTextContent" style=" color:black; font-weight:bold; padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;"> DO NOT REPLY TO THIS EMAIL </td>
                                                    </tr>
                                                </tbody>
                                            </table>

                                        </td>
                                    </tr>
                                    
                                   <tr>
                                        <td valign="top" class="preheaderContainer" style="padding-top:9px;">
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="366" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;">

                                                                            Your event has been approved!
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                            <table align="right" border="0" cellpadding="0" cellspacing="0" width="197" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;">

                                                                            <!--<a href="*|ARCHIVE|*" target="_blank">View this email in your browser</a>-->
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <!-- // END PREHEADER -->
                            </td>
                        </tr>
                        <tr>
                            <td align="center" valign="top">
                                <!-- BEGIN HEADER // -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateHeader">
                                    <tr>
                                        <td valign="top" class="headerContainer">
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnImageBlock">
                                                <tbody class="mcnImageBlockOuter">
                                                    <tr>
                                                        <td valign="top" style="padding:9px" class="mcnImageBlockInner">
                                                            <table align="left" width="100%" border="0" cellpadding="0" cellspacing="0" class="mcnImageContentContainer">
                                                                <tbody>
                                                                    <tr>
                                                                        <td class="mcnImageContent" valign="top" style="padding-right: 9px; padding-left: 9px; padding-top: 0; padding-bottom: 0;">

                                                                            <a href="http://grubclub.com/?utm_source=mailchimp&amp;utm_medium=banner_top&amp;utm_campaign=banner_top" title="" class="" target="_blank">
                                                                                <img align="left" alt="" src="[serverURL]Content/images/email/Logo-Jess.png" width="564" style="max-width:1408px; padding-bottom: 0; display: inline !important; vertical-align: bottom;" class="mcnImage" />
                                                                            </a>

                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnImageBlock">
                                                <tbody class="mcnImageBlockOuter">
                                                    <tr>
                                                        <td valign="top" style="padding:9px" class="mcnImageBlockInner">
                                                            <table align="left" width="100%" border="0" cellpadding="0" cellspacing="0" class="mcnImageContentContainer">
                                                                <tbody>
                                                                    <tr>
                                                                        <td class="mcnImageContent" valign="top" style="padding-right: 9px; padding-left: 9px; padding-top: 0; padding-bottom: 0;">


                                                                            <img align="left" alt="Time is nearly up!" src="[serverURL]Content/images/email/accept-GrubClub.jpg" width="564" style="max-width:981px; padding-bottom: 0; display: inline !important; vertical-align: bottom;" class="mcnImage" />


                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">
                                                <tbody class="mcnDividerBlockOuter">
                                                    <tr>
                                                        <td class="mcnDividerBlockInner" style="padding: 18px;">
                                                            <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">
                                                                <tbody>
                                                                    <tr>
                                                                        <td>
                                                                            <span></span>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">

                                                                            <div><span style="color:#000000"><strong><span style="font-size:24px">Hi&nbsp;[fname]!&nbsp;</span></strong></span></div>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">

                                                                            <div>
                                                                                <span style="font-size:16px">
                                                                                    <strong>
                                                                                        Your Grub Club is now live!<br />
                                                                                        <br />
                                                                                        Here is your booking link: <a href="[eventURL]" target="_blank">[eventURL]</a>
                                                                                    </strong>
                                                                                </span>
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">
                                                <tbody class="mcnDividerBlockOuter">
                                                    <tr>
                                                        <td class="mcnDividerBlockInner" style="padding: 18px;">
                                                            <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">
                                                                <tbody>
                                                                    <tr>
                                                                        <td>
                                                                            <span></span>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">

                                                                            <font color="#ff5e00"><span style="font-size:18px"><strong>The next steps:</strong></span></font><br />
                                                                            <br />
                                                                            Time to start <strong>spreading the word</strong> about your Grub Club + <strong>monitoring any bookings</strong> coming in:
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnImageGroupBlock">
                                                <tbody class="mcnImageGroupBlockOuter">

                                                    <tr>
                                                        <td valign="top" style="padding:9px" class="mcnImageGroupBlockInner">

                                                            <table align="left" width="273" border="0" cellpadding="0" cellspacing="0" class="mcnImageGroupContentContainer">
                                                                <tbody>
                                                                    <tr>
                                                                        <td class="mcnImageGroupContent" valign="top" style="padding-left: 9px; padding-top: 0; padding-bottom: 0;">

                                                                            <a href="http://host-marketing-tips.grubclub.com/" title="" class="" target="_blank">
                                                                                <img alt="Marketing Tips" src="[serverURL]Content/images/email/marketing-tips.jpg" width="264" style="max-width:500px; padding-bottom: 0;" class="mcnImage" />
                                                                            </a>

                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                            <table align="right" width="273" border="0" cellpadding="0" cellspacing="0" class="mcnImageGroupContentContainer">
                                                                <tbody>
                                                                    <tr>
                                                                        <td class="mcnImageGroupContent" valign="top" style="padding-right: 9px; padding-top: 0; padding-bottom: 0;">

                                                                            <a href="http://admin-tips.grubclub.com/" title="" class="" target="_blank">
                                                                                <img alt="" src="[serverURL]Content/images/email/admin-tips.jpg" width="264" style="max-width:500px; padding-bottom: 0;" class="mcnImage" />
                                                                            </a>

                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>

                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">
                                                <tbody class="mcnDividerBlockOuter">
                                                    <tr>
                                                        <td class="mcnDividerBlockInner" style="padding: 18px;">
                                                            <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">
                                                                <tbody>
                                                                    <tr>
                                                                        <td>
                                                                            <span></span>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">

                                                                            If you haven''t already, please now take the time to complete your <strong>public profile </strong>and your <strong>long bio</strong>.<br />
                                                                            <br />
                                                                            You can do so here: <a href="[profileURL]" target="_blank">[profileURL]</a>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnDividerBlock">
                                                <tbody class="mcnDividerBlockOuter">
                                                    <tr>
                                                        <td class="mcnDividerBlockInner" style="padding: 18px;">
                                                            <table class="mcnDividerContent" border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top-width: 1px;border-top-style: solid;border-top-color: #999999;">
                                                                <tbody>
                                                                    <tr>
                                                                        <td>
                                                                            <span></span>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <!-- // END HEADER -->
                            </td>
                        </tr>
                        <tr>
                            <td align="center" valign="top">
                                <!-- BEGIN BODY // -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateBody">
                                    <tr>
                                        <td valign="top" class="bodyContainer">
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;">

                                                                            <img align="none" height="43" src="[serverURL]Content/images/email/grub-love.png" style="width: 150px; height: 43px; margin: 0px;" width="150" />
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table><table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="600" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right: 18px; padding-bottom: 9px; padding-left: 18px;"></td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <!-- // END BODY -->
                            </td>
                        </tr>
                        <tr>
                            <td align="center" valign="top">
                                <!-- BEGIN FOOTER // -->
                                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateFooter">
                                    <tr>
                                        <td valign="top" class="footerContainer" style="padding-bottom:9px;">
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%" class="mcnTextBlock">
                                                <tbody class="mcnTextBlockOuter">
                                                    <tr>
                                                        <td valign="top" class="mcnTextBlockInner">

                                                            <table align="left" border="0" cellpadding="0" cellspacing="0" width="282" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;">

                                                                            <br />
                                                                            Copyright &copy; 2015 GrubClub, All rights reserved.&nbsp;<br />
                                                                            Our mailing address is:&nbsp;<br />
                                                                            <a href="mailto:eat@grubclub.com" target="_blank">eat@grubclub.com</a>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                            <table align="right" border="0" cellpadding="0" cellspacing="0" width="282" class="mcnTextContentContainer">
                                                                <tbody>
                                                                    <tr>

                                                                        <td valign="top" class="mcnTextContent" style="padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;">

                                                                            <div style="text-align: right;">
                                                                                <br />
                                                                                www.grubclub.com<br />
                                                                                <span style="line-height:20.7999992370605px; text-align:right">[OfficeAddress]</span>
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>

                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <!-- // END FOOTER -->
                            </td>
                        </tr>
                    </table>
                    <!-- // END TEMPLATE -->
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