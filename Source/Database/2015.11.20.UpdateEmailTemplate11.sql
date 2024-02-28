-- Author:	Supraja 
-- Summary:	This updates the Email Template for guest booking confirmation email

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.20.UpdateEmailTemplate11'

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
WHERE [Id] = 11

INSERT INTO [SupperClub].[dbo].[EmailTemplate]
           ([Id], [Subject], [Html],
           [Body]
           )
     VALUES
           (11, 'Your Grub Club Booking', 'True',
          N' <!DOCTYPE HTML PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html style="height: 100%; min-height: 100%; background-color: #eae5dd !important; font-family: ''Source Sans Pro'', sans-serif !important;">
<head>
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <title>Grubclub</title>

    <script src="grubclub.com/bower_components/svg4everybody/svg4everybody.min.js"></script>

    <style type="text/css">
        body {
            width: 100% !important;
            min-width: 100%;
            -webkit-text-size-adjust: 100%;
            -ms-text-size-adjust: 100%;
            margin: 0;
            padding: 0;
        }

        .ExternalClass {
            width: 100%;
        }

        .ExternalClass {
            line-height: 100%;
        }

        #backgroundTable {
            margin: 0;
            padding: 0;
            width: 100% !important;
            line-height: 100% !important;
        }

        img {
            outline: none;
            text-decoration: none;
            -ms-interpolation-mode: bicubic;
            width: auto;
            max-width: 100%;
            float: left;
            clear: both;
            display: block;
        }

        body {
            color: #222222;
            font-family: "Helvetica", "Arial", sans-serif;
            font-weight: normal;
            padding: 0;
            margin: 0;
            text-align: left;
            line-height: 1.3;
        }

        body {
            font-size: 14px;
            line-height: 19px;
        }

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
            background: #d0d0d0 !important;
            color: #555;
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

        body.outlook p {
            display: inline !important;
        }

        body {
            color: #a9a9a8;
        }

        @font-face {
            font-family: ''Source Sans Pro'';
            font-style: normal;
            font-weight: 300;
            src: local(''Source Sans Pro Light''), local(''SourceSansPro-Light''), url(''http://fonts.gstatic.com/s/sourcesanspro/v9/toadOcfmlt9b38dHJxOBGMw1o1eFRj7wYC6JbISqOjY.ttf'') format(''truetype'');
        }

        @font-face {
            font-family: ''Source Sans Pro'';
            font-style: normal;
            font-weight: 400;
            src: local(''Source Sans Pro''), local(''SourceSansPro-Regular''), url(''http://fonts.gstatic.com/s/sourcesanspro/v9/ODelI1aHBYDBqgeIAH2zlNzbP97U9sKh0jjxbPbfOKg.ttf'') format(''truetype'');
        }

        @font-face {
            font-family: ''Source Sans Pro'';
            font-style: normal;
            font-weight: 600;
            src: local(''Source Sans Pro Semibold''), local(''SourceSansPro-Semibold''), url(''http://fonts.gstatic.com/s/sourcesanspro/v9/toadOcfmlt9b38dHJxOBGNNE-IuDiR70wI4zXaKqWCM.ttf'') format(''truetype'');
        }

        @font-face {
            font-family: ''Source Sans Pro'';
            font-style: normal;
            font-weight: 700;
            src: local(''Source Sans Pro Bold''), local(''SourceSansPro-Bold''), url(''http://fonts.gstatic.com/s/sourcesanspro/v9/toadOcfmlt9b38dHJxOBGLsbIrGiHa6JIepkyt5c0A0.ttf'') format(''truetype'');
        }

        @media only screen and (max-width: 600px) {
            table[class="body"] img {
                width: auto !important;
                height: auto !important;
            }

            table[class="body"] center {
                min-width: 0 !important;
            }

            table[class="body"] .container {
                width: 95% !important;
            }

            table[class="body"] .row {
                width: 100% !important;
                display: block !important;
            }

            table[class="body"] .wrapper {
                display: block !important;
                padding-right: 0 !important;
            }

            table[class="body"] .columns {
                table-layout: fixed !important;
                float: none !important;
                width: 100% !important;
                padding-right: 0px !important;
                padding-left: 0px !important;
                display: block !important;
            }

            table[class="body"] .column {
                table-layout: fixed !important;
                float: none !important;
                width: 100% !important;
                padding-right: 0px !important;
                padding-left: 0px !important;
                display: block !important;
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
                width: 8.33333% !important;
            }

            table[class="body"] .column td.one {
                width: 8.33333% !important;
            }

            table[class="body"] .columns td.two {
                width: 16.66667% !important;
            }

            table[class="body"] .column td.two {
                width: 16.66667% !important;
            }

            table[class="body"] .columns td.three {
                width: 25% !important;
            }

            table[class="body"] .column td.three {
                width: 25% !important;
            }

            table[class="body"] .columns td.four {
                width: 33.33333% !important;
            }

            table[class="body"] .column td.four {
                width: 33.33333% !important;
            }

            table[class="body"] .columns td.five {
                width: 41.66667% !important;
            }

            table[class="body"] .column td.five {
                width: 41.66667% !important;
            }

            table[class="body"] .columns td.six {
                width: 50% !important;
            }

            table[class="body"] .column td.six {
                width: 50% !important;
            }

            table[class="body"] .columns td.seven {
                width: 58.33333% !important;
            }

            table[class="body"] .column td.seven {
                width: 58.33333% !important;
            }

            table[class="body"] .columns td.eight {
                width: 66.66667% !important;
            }

            table[class="body"] .column td.eight {
                width: 66.66667% !important;
            }

            table[class="body"] .columns td.nine {
                width: 75% !important;
            }

            table[class="body"] .column td.nine {
                width: 75% !important;
            }

            table[class="body"] .columns td.ten {
                width: 83.33333% !important;
            }

            table[class="body"] .column td.ten {
                width: 83.33333% !important;
            }

            table[class="body"] .columns td.eleven {
                width: 91.66667% !important;
            }

            table[class="body"] .column td.eleven {
                width: 91.66667% !important;
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

        @media (max-width: 600px) {
            .responsive-text--left {
                text-align: left !important;
            }
        }

        @media (min-width: 601px) {
            .header-main--right-align {
                text-align: right !important;
                margin-right: -20px !important;
            }
        }
    </style>
</head>
<body style="width: 100% !important; min-width: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; color: #a9a9a8; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0;">


    <table class="body" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; height: 100%; width: 100%; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; background-color: #eae5dd !important; margin: 0; padding: 0;" bgcolor="#eae5dd !important">
        <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
            <td class="center" align="center" valign="top" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center !important; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;">
                <center style="width: 100%; min-width: 580px; font-family: ''Source Sans Pro'', sans-serif !important;">
                    <table class="container" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; min-width: 320px; margin: 0 auto; padding: 0 10px;">
                        <tr style="vertical-align: top; text-align: center; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="center">
                            <td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="center" valign="top">
							DO NOT REPLY TO THIS EMAIL
                                <table class="row" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0px;">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pt0" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0px 10px;" align="left" valign="top">
                                                        <!--<a href="" target="_blank" class="text--extra-small clr--gray-dark" style="color: #3d4143 !important; text-decoration: none; font-family: ''Source Sans Pro'', sans-serif !important; font-size: 10px !important;">View this email in your browser ></a>-->
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row header-main bgr--white mb-" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: center; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 10px !important; background-color: white !important; padding: 0px;" bgcolor="white !important">

                                    <tr style="vertical-align: top; text-align: center; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="center">
                                        <td class="wrapper pt0" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 0px 0px;" align="center" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: center; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="center">
                                                    <td class="pt- pl left-text-pad" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="center" valign="top">
                                                        <img src="http://grubclub.com//Content/images/email/logo.png" alt="" style=" margin-left: 200px !important; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; float: none; clear: both; display: block; font-family: ''Source Sans Pro'', sans-serif !important;" align="center" />
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;"></tbody>
                                            </table>
                                        </td>


                                    </tr>




                                </table>
                            </td>
                        </tr>
                        <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                            <td style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top">
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl pt-" colspan="3" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="text--20 light clr--gray-dark lh-1-2 mb-" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 1.2 !important; font-size: 20px !important; margin: 0 0 10px; padding: 0;" align="left">Hi  [userName], Here''s the confirmation of your Grub Club!</p>
                                                        <a href="[eventUrl]" class="block " style="color: #2ba6cb; text-decoration: none; font-family: ''Source Sans Pro'', sans-serif !important; display: block !important;" target="_blank">
                                                            <h2 class="text--28 light clr--orange mb-" style="color: #f47400 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 1; word-break: normal; font-size: 28px !important; margin: 0 0 10px; padding: 0;" align="left">[eventName]</h2>
                                                        </a>
                                                        <p class="text--middle light clr--gray-dark lh-1-2" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 1.2 !important; font-size: 16px !important; margin: 0; padding: 0;" align="left">
                                                            Hosted by
                                                            <a target="_blank" href="[serverURL][hostUrl]" class="block " style="text-decoration: none; font-family: ''Source Sans Pro'', sans-serif !important;" target="_blank">
                                                                [grubClubName]
                                                            </a>
                                                        </p>
                                                        </a>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                </tr>
                                            </table>
                                        </td>

                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper pl-" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 10px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="left-text-pad" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top">
                                                        <a href="[eventUrl]" target="_blank">     <img src="grubclub.com/Content/images/Events/[imageUrl]" alt="" class="full--width float-none max--width-100" style="outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: 100% !important; max-width: 100% !important; float: none !important; clear: both; display: block; font-family: ''Source Sans Pro'', sans-serif !important;" align="none !important" /></a>
                                                        </a></td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;"></tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper last pl-" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px 10px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="left-text-pad pr- pt" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 20px 10px 10px;" align="left" valign="top">
                                                        <p class="text--middle opacity-7 mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;" align="left">[eventDateTime]</p><br/>
														<p class="text--middle opacity-7 mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;" align="left"> <a href="http://www.google.com/calendar/event?action=TEMPLATE&text=[eventName]&dates=[calStartDateTime]/[calEndDateTime]&details=For+details,+please+visit+[eventUrl]&location=[eventAddress],[eventAddress2],[eventCity][eventPostCode]&trp=false&sprop=&sprop=name:" target="_blank" rel="nofollow" style="color:#ff7400; text-decoration: none;">Add to my calendar</a> </p>
                                                        <p class="text--middle opacity-7 mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;" align="left">[numberTickets]</p>
                                                        [vouchercode]
                                                        <p class="text--30 clr--gray-dark bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 30px !important; margin: 0; padding: 0;" align="left">[currency][totalPaid]</p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl pt" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 20px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="hr hr--dotted mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; background-color: transparent !important; height: 1px !important; width: 100% !important; border-bottom-color: #dadbdc !important; border-bottom-width: 3px !important; border-bottom-style: dotted !important; outline: none !important; margin: 0 0 10px; padding: 0;" align="left"></p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left"></tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper pt0" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 0px 0px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pt- pl left-text-pad" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                        <h2 class="clr--gray-dark light" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 1; word-break: normal; font-size: 26px; margin: 0; padding: 0;" align="left">Your Order</h2>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;"></tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pt- pl right-text-pad pr0" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 10px 20px;" align="left" valign="top">
                                                        <p class="clr--gray text--normal bold header-main--right-align pr0 pl-" style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0 0 0 10px;" align="left">Order Number: [bookingReference]</p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="three columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 130px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;">
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray-dark text--normal bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">Name</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray text--normal " style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[userName] </p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="three columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 130px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;">
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray-dark text--normal bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">Ticket</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray text--normal " style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[eventName]</p>
                                                            </a>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="three columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 130px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;">
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray-dark text--normal bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">Quantity</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray text--normal " style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[quantity]</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray-dark text--normal bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">Total</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px;" align="left" valign="top">
                                            <table class="three columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 130px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;">
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray-dark text--normal bold" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 700 !important; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0; width: 120px;" align="left">Price Per Person</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray text--normal " style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[currency][eventCost]</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                        <td class="pt- pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;" align="left" valign="top">
                                                            <p class="clr--gray text--normal " style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[currency][totalPaid]</p>
                                                        </td>
                                                        <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl pt" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 20px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="clr--gray text--normal center mb-" style="color: #a9a9a8 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center !important; line-height: 19px; font-size: 14px !important; margin: 0 0 10px; padding: 0;" align="center !important">Charged to Visa: XXXX XXXX XXXX [ccLastDigits]</p>
                                                        
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left"></tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl pt" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 20px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="hr hr--dotted mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; background-color: transparent !important; height: 1px !important; width: 100% !important; border-bottom-color: #dadbdc !important; border-bottom-width: 3px !important; border-bottom-style: dotted !important; outline: none !important; margin: 0 0 10px; padding: 0;" align="left"></p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left"></tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper pl-" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 10px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="left-text-pad" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px 10px;" align="left" valign="top">
                                                        <h2 class="clr--gray-dark light mb" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 1; word-break: normal; font-size: 26px; margin: 0 0 20px; padding: 0;" align="left">Your Event</h2>
                                                        <p class="text--normal clr-gray mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 10px; padding: 0;" align="left">[eventDateTime] </p>
                                                        <p class="text--normal clr-gray" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[eventAddress] </p>
                                                        <p class="text--normal clr-gray" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[eventAddress2] </p>
                                                        <p class="text--normal clr-gray" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;" align="left">[eventCity]</p>
                                                        <p class="text--normal clr-gray mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 10px; padding: 0;" align="left">[eventPostCode]</p>
                                                        <p class="text--normal clr-gray mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;" align="left">Booking for [numberPeople]</p>
                                                        [menuItems]
                                                        [byob]
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tbody style="font-family: ''Source Sans Pro'', sans-serif !important;"></tbody>
                                            </table>
                                        </td>
                                        <td class="wrapper last pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 0px 20px;" align="left" valign="top">
                                            <table class="six columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="left-text-pad pl0 pr-" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 10px 10px 0;" align="left" valign="top">
                                                        <a href="http://maps.google.co.uk/?q=[eventAddress] [eventAddress2] [eventCity] [eventPostCode]">
                                                            <img src="https://maps.googleapis.com/maps/api/staticmap?size=300x160&maptype=roadmap&markers=size:mid%7Ccolor:red%7C[eventAddress],[eventAddress2],[eventCity] [eventPostCode]" style="position: relative; overflow: hidden; transform: translateZ(0px); background-color: rgb(229, 227, 223); margin-top:-30px;margin-bottom: 10px !important; " align="none !important" />
                                                        </a>

                                                        <p class="text--normal clr-gray mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;" align="left"><a target="_blank" href="http://maps.google.co.uk/?q=[eventAddress] [eventAddress2] [eventCity] [eventPostCode]" class="text--small opacity-7  clr--gray-dark underline" style="color: #3d4143 !important; text-decoration: underline !important; font-family: ''Source Sans Pro'', sans-serif !important; font-size: 12px !important; opacity: 0.7;">View on Maps</a></p>
                              </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="hr hr--dotted mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; background-color: transparent !important; height: 1px !important; width: 100% !important; border-bottom-color: #dadbdc !important; border-bottom-width: 3px !important; border-bottom-style: dotted !important; outline: none !important; margin: 0 0 10px; padding: 0;" align="left"></p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left"></tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: center; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: center; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="center">
                                        <td>
                                            <p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin-left:40px;" align="center">Feel free to arrive anytime from [eventStartTime]. The venue closes at [eventEndTime].</p>
                                            <p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0;" align="center"><b>Special Requirements:</b> [bookingRequirements]</p>
                                            <p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0;" align="center"><b>Alcohol Policy:</b> [alcoholPolicy]</p>
                                             [comments]
                                            <p class="mb" style="color: #f47400; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0; " align="center">Got Questions? Email: <a href="mailto:eat@grubclub.com" target="_blank">eat@grubclub.com</a></p>
											<p class="mb" style="color: #f47400; font-family: ''Source Sans Pro'', sans-serif !

important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; 

margin: 0 0 20px; padding: 0; " align="center">Need to cancel? You can see our <span 

class="il">Cancellation</span> <span class="il">policy</span> <a 

href="http://grubclub.com/faqs" target="_blank">here</a></p>
                                        </td>

                                    </tr>
                                </table>
                                <table class="row bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 0px 10px 20px;" align="left" valign="top">
                                                        <p class="hr hr--dotted mb-" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; background-color: transparent !important; height: 1px !important; width: 100% !important; border-bottom-color: #dadbdc !important; border-bottom-width: 3px !important; border-bottom-style: dotted !important; outline: none !important; margin: 0 0 10px; padding: 0;" align="left"></p>
                                                    </td>
                                                    <td class="expander" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;" align="left" valign="top"></td>
                                                </tr>
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left"></tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                            <td class="last" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0px 0 0;" align="left" valign="top">

                                <table class="row footer-main bgr--white" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; font-family: ''Source Sans Pro'', sans-serif !important; background-color: white !important; padding: 0px;" bgcolor="white !important">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 20px;" align="left" valign="top">
                                            <table class="four columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0;" align="left">Join the conversation</p>

                                                <div>
                                                    <div>
                                                        <a target="_blank" href="http://www.twitter.com/Grub_Club">   <img src="http://grubclub.com/content/images/social_icons/twitter1.png" alt="" class="max--width-100 block mb" style="outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width:15%; max-width: 100% !important; float: left; clear: both; display: block !important; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 20px !important; border: none;" align="left" /></a>

                                                        </a>
                                                    </div>   <div style="float:left;">
                                                        <a target="_blank" href="http://www.facebook.com/GrubClub1"><img src="http://grubclub.com/content/images/social_icons/facebook1.png" alt="" class="max--width-100 block mb" style="outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width:75%; max-width: 100% !important; float: left; clear: both; display: block !important; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 20px !important; margin-left:5px;  border: none;" /></a>

                                                        </a>
                                                    </div>   <div style="float:left;">
                                                        <a target="_blank" href="http://instagram.com/grub_club/"> <img src="http://grubclub.com/content/images/social_icons/instagram1.png" alt="" class="max--width-100 block mb" style="outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: 75%; max-width: 100% !important; float: left; clear: both; display: block !important; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 20px !important; margin-left: 5px; border: none;" /></a>

                                                        </a>
                                                    </div>
                                                </div>  
                                            </table>
                                        </td>
                                        <td class="wrapper pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 20px;" align="left" valign="top">
                                            <table class="four columns pt0" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <a target="_blank" style="text-decoration: none;" href="[serverURL]pop-up-restaurants"><p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0;" align="left">Hungry for more?</p></a>
                                                <a href="[serverURL]pop-up-restaurants" target="_blank" class="clr--orange bold text--normal block mb" style="color: #f47400 !important; text-decoration: none; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 20px !important; font-size: 14px !important; font-weight: 700 !important; display: block !important;">View more Grub Clubs ></a>
                                            </table>
                                        </td>
                                        <td class="wrapper pl" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 20px;" align="left" valign="top">
                                            <table class="four columns last pt0" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 180px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <a style="text-decoration: none;" target="_blank" href="[serverURL]refer-a-friend"><p class="mb" style="color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0 0 20px; padding: 0;" align="left"> Spread the Word!</p></a>
                                                <a href="[serverURL]refer-a-friend" target="_blank" class="clr--orange bold text--normal block mb" style="color: #f47400 !important; text-decoration: none; font-family: ''Source Sans Pro'', sans-serif !important; margin-bottom: 20px !important; font-size: 14px !important; font-weight: 700 !important; display: block !important;">Invite your friends ></a>
                                            </table>
                                        </td>
                                    </tr>
                                </table>

                                <table class="row footer-main" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0px;">
                                    <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                        <td class="wrapper" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0 0px 0px;" align="left" valign="top">
                                            <table class="twelve columns" style="border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; font-family: ''Source Sans Pro'', sans-serif !important; margin: 0 auto; padding: 0;">
                                                <tr style="vertical-align: top; text-align: left; font-family: ''Source Sans Pro'', sans-serif !important; padding: 0;" align="left">
                                                    <td class="twelve sub-columns" style="word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; color: #222222; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0px 10px 10px 0px;" align="left" valign="top">
                                                        <p class="center clr--gray-dark text--small" style="color: #3d4143 !important; font-family: ''Source Sans Pro'', sans-serif !important; font-weight: normal; text-align: center !important; line-height: 19px; font-size: 12px !important; margin: 0; padding: 0;" align="center !important">Copyright &copy; Grub Club 2015 <span class="inline-block pl pr" style="font-family: ''Source Sans Pro'', sans-serif !important; padding-left: 20px !important; padding-right: 20px !important; display: inline-block !important;">|</span> Net-Works, 25-27 Horsell Road, London N5 1XL</p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
								 </td>
                        </tr>
                    </table>
                </center>
            </td>
        </tr>
    </table>

</body>
</html>
    '           )

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