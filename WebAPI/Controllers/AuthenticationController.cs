using DomainLayer.Data;
using DomainLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Service_Layer.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Service_Layer.ICustomServices;
using ConfigurationManager = WebAPI.ConfigurationManager;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mail;
        public AuthenticationController(IUnitOfWork unitOfWork, IMailService mail)
        {
            _unitOfWork = unitOfWork;
            _mail = mail;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login user)
        {
            if (user is null)
            {
                return BadRequest("Invalid user request!!!");
            }
            UserDetail obj = _unitOfWork.User.GetAuthenticatedUserDetail(user);
            if (obj != null)
            {
                if (user.UserName.ToLower() == obj.Email.ToLower() && user.Password == CommonMethods.Decryptword(obj.Password))
                {
                    var claims = new[]{
                                        new Claim("Id",obj.Id.ToString()),
                                        new Claim("Name",obj.Name),
                                        new Claim("Email",obj.Email),
                                        new Claim("TokenFor","UserAuth")
                                        };
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                    var tokeOptions = new JwtSecurityToken(
                        issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                        audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                        claims: claims,
                        expires: DateTime.Now.AddHours(24),
                        signingCredentials: signinCredentials
                    );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                    return Ok(new JWTTokenResponse { Token = tokenString });
                }
            }
            //return Unauthorized();
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Response = "Invalid User Id And Password.", Status = "Failure" });
        }

        [HttpGet("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string EmailId)
        {
            if (EmailId == null || EmailId == "" || !CommonMethods.IsValidEmail(EmailId))
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Response = "Invalid Email Id.", Status = "Failure" });
            }
            else
            {
                UserDetail obj = _unitOfWork.User.GetT(x => x.Email == EmailId && x.IsActive == true && x.IsDeleted == false);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Response = "No User Found.", Status = "Failure" });
                }
                else
                {
                    bool result = await SendResetPasswordMail(obj);
                    if (result)
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Response = "Mail Sent Successfully.", Status = "Success" });
                    }
                    else
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Response = "An error occured. The Mail could not be sent.", Status = "Failure" });
                    }
                }
            }

        }
        [Authorize]
        [HttpGet("GetEmailByToken")]
        public async Task<IActionResult> GetEmailByToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                if (UserId!=null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Response = (_unitOfWork.User.GetT(x => x.Id == UserId)).Email.ToString(), Status = "Success" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Response = "User Does Not Exist.", Status = "Failure" });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Response = "User Does Not Exist.", Status = "Failure" });
        }

        [Authorize]
        [HttpPost("ResetPassword")]
        public ActionResult ResetPassword([FromBody] string NewPassword)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                string TokenFor = userClaims.FirstOrDefault(x => x.Type == "TokenFor").Value;
                if (TokenFor == "ResetPassword")
                {
                    //UserDetail obj=_unitOfWork.User.GetT(x=>x.Id==Convert.ToInt32(UserId));
                    NewPassword = CommonMethods.Encryptword(NewPassword);
                    _unitOfWork.User.ResetPassword(UserId,NewPassword);
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Response = "Password Changed Successfully.", Status = "Success" });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Response = "Password Changed Successfully.", Status = "Success" });
        }

        private async Task<bool> SendResetPasswordMail(UserDetail obj)
        {
            try
            {
                var claims = new[]{
                                        new Claim("Id",obj.Id.ToString()),
                                         new Claim("TokenFor","ResetPassword")
                                        };
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

                bool result = true;
                List<string> to = new List<string>();
                List<string> bcc = new List<string>();
                List<string> cc = new List<string>();
                to.Add(obj.Email);
                bcc.Add("jaydeep.sahu@radixweb.com");
                cc.Add("prashansa.khandelwal@radixweb.com");
                string subject = "Rx-Splitter! Forgot Password Link";
                string body = "<!doctype html> <html lang=\"en-US\"> <head> <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" /> <title>Reset Password Email Template</title> <meta name=\"description\" content=\"Reset Password Email Template.\"> <style type=\"text/css\"> a:hover {text-decoration: underline !important;} </style> </head> <body marginheight=\"0\" topmargin=\"0\" marginwidth=\"0\" style=\"margin: 0px; background-color: #f2f3f8;\" leftmargin=\"0\"> <!--100% body table--> <table cellspacing=\"0\" border=\"0\" cellpadding=\"0\" width=\"100%\" bgcolor=\"#f2f3f8\" style=\"@import url(https://fonts.googleapis.com/css?family=Rubik:300,400,500,700|Open+Sans:300,400,600,700); font-family: 'Open Sans', sans-serif;\"> <tr> <td> <table style=\"background-color: #f2f3f8; max-width:670px;  margin:0 auto;\" width=\"100%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <a href=\"https://radixattendance.azurewebsites.net/\" title=\"logo\" target=\"_blank\"> <img width=\"60\" src=\"https://i.ibb.co/hL4XZp2/android-chrome-192x192.png\" title=\"logo\" alt=\"logo\"> </a> </td> </tr> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td> <table width=\"95%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:670px;background:#fff; border-radius:3px; text-align:center;-webkit-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);-moz-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);box-shadow:0 6px 18px 0 rgba(0,0,0,.06);\"> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> <tr> <td style=\"padding:0 35px;\"> <h1 style=\"color:#1e1e2d; font-weight:500; margin:0;font-size:32px;font-family:'Rubik',sans-serif;\">You have requested to reset your password</h1> <span style=\"display:inline-block; vertical-align:middle; margin:29px 0 26px; border-bottom:1px solid #cecece; width:100px;\"></span> <p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\"> We cannot simply send you your old password. A unique link to reset your password has been generated for you. To reset your password, click the following link and follow the instructions. </p> <a href=\"https://radixattendance.azurewebsites.net/#/ResetPassword/" + tokenString + "\"+ tokenString + \"'\" style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Reset Password</a> </td> </tr> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> </table> </td> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <p style=\"font-size:14px; color:rgba(69, 80, 86, 0.7411764705882353); line-height:18px; margin:0 0 0;\">&copy; <strong>https://radixattendance.azurewebsites.net</strong></p> </td> </tr> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> </table> </td> </tr> </table> <!--/100% body table--> </body> </html>";
                //string body = "<a href='https://radixattendance.azurewebsites.net/#/ResetPassword/'"+ tokenString + "'><input type='button' value='Reset Password' /></a>";
                string from = "svni7071@gmail.com";
                string displayName = obj.Name;
                MailData mailData = new MailData(to, subject, body, from, displayName);
                result = await _mail.SendAsync(mailData, new CancellationToken());
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
