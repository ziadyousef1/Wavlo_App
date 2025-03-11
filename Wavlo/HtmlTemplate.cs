namespace Wavlo
{
    public class HtmlTemplate
    {
        public static string GetVerificationCodeEmailTemplate(string verificationCode)
        {
            var htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }
        .header {
            text-align: center;
            padding: 10px 0;
            border-bottom: 1px solid #dddddd;
        }
        .header h1 {
            margin: 0;
            font-size: 24px;
            color: #333333;
        }
        .content {
            padding: 20px 0;
        }
        .content p {
            font-size: 16px;
            color: #666666;
            line-height: 1.5;
        }
        .verification-code {
            font-size: 24px;
            font-weight: bold;
            color: #333333;
            text-align: center;
            margin: 20px 0;
        }
        .footer {
            text-align: center;
            padding: 10px 0;
            border-top: 1px solid #dddddd;
            font-size: 12px;
            color: #999999;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Verification Code</h1>
        </div>
        <div class='content'>
            <p>Dear User,</p>
            <p>Your verification code is:</p>
            <div class='verification-code'>{{VERIFICATION_CODE}}</div>
            <p>Please use this code to verify your account. This code is valid for 10 minutes.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2023 Your Company. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
            return htmlTemplate.Replace("{{VERIFICATION_CODE}}", verificationCode);
        }

        public static string GetPasswordResetEmailTemplate(string verificationCode)
        {
            var htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }
        .header {
            text-align: center;
            padding: 10px 0;
            border-bottom: 1px solid #dddddd;
        }
        .header h1 {
            margin: 0;
            font-size: 24px;
            color: #333333;
        }
        .content {
            padding: 20px 0;
        }
        .content p {
            font-size: 16px;
            color: #666666;
            line-height: 1.5;
        }
        .verification-code {
            font-size: 24px;
            font-weight: bold;
            color: #333333;
            text-align: center;
            margin: 20px 0;
        }
        .footer {
            text-align: center;
            padding: 10px 0;
            border-top: 1px solid #dddddd;
            font-size: 12px;
            color: #999999;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Verification Code</h1>
        </div>
        <div class='content'>
            <p>Dear User,</p>
            <p>Your password reset verification code is:</p>
            <div class='verification-code'>{{VERIFICATION_CODE}}</div>
            <p>Please use this code to reset your password. This code is valid for 10 minutes.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2023 Your Company. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
            return htmlTemplate.Replace("{{VERIFICATION_CODE}}", verificationCode);
        }
    }
}
