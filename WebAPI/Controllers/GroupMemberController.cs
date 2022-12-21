using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Service_Layer.ICustomServices;
using Service_Layer.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class GroupMemberController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMailService _mail;
        public GroupMemberController(IUnitOfWork unitOfWork, IMapper mapper, IMailService mail)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mail = mail;
        }

        [HttpGet("GetAllMembersByGroupId/{Id}")]
        public IActionResult GetAllMembersByGroupId(int Id)
        {
            var obj = _unitOfWork.GroupMember.GetByExpression(x => x.GroupId == Id);
            return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
        }

        [HttpPost("CreateGroupMember/{GroupId}")]
        public IActionResult CreateGroupMember(int GroupId, [FromBody] JsonElement lstMember)
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(lstMember);
                List<GroupMember> lstGroupMember = JsonConvert.DeserializeObject<List<GroupMember>>(json);
                //List<GroupMember> lstGroupMember =new List<GroupMember>();

                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity != null && lstGroupMember != null && lstGroupMember.Count > 0)
                {
                    var userClaims = identity.Claims;
                    Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                    bool res = false;
                    foreach (var member in lstGroupMember)
                    {
                        member.GroupId = GroupId;
                        var mappedMember=_mapper.Map<GroupMember>(member);
                        var existUser=_unitOfWork.User.GetT(x=>x.Email== mappedMember.Email);
                        if(existUser!=null)
                        {
                            mappedMember.UserId = existUser.Id;
                        }
                        else {
                            res = _unitOfWork.GroupMember.Insert(mappedMember);
                        }
                        SendInvitationMail(mappedMember.Email, (UserDetail)_unitOfWork.User.GetByExpression(x=>x.Id==UserId).FirstOrDefault());
                    }
                    _unitOfWork.Save();
                    if (res)
                    {
                        _unitOfWork.Save();
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = _unitOfWork.GroupMember.GetByExpression(x=>x.GroupId==GroupId) });
                    }
                    else
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "Operation Failed." });
                    }
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "The User Data given by you is totally empty.." });
                }
            }
            catch(Exception ex)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "Member Data Not Found." });
            }
        }
        [HttpPut(nameof(UpdateGroupMember))]
        public IActionResult UpdateGroupMember(GroupMember groupMember)
        {
            if (groupMember != null)
            {
                groupMember.UpdatedOn= DateTime.Now;
                _unitOfWork.GroupMember.Update(groupMember);
                _unitOfWork.Save();
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The Group Member Data updated Successfully." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The Group Data given by you is totally empty.." });
            }
        }

        [HttpDelete("DeleteGroupMember/{memberId}")]
        public IActionResult DeleteGroupMember(int memberId)
        {
            var groupMember = _unitOfWork.GroupMember.Get(memberId);
            if (groupMember != null)
            {
                bool res = _unitOfWork.GroupMember.Delete(groupMember);
                if (res)
                {
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The Group Member Data deleted Successfully." });
                }
                return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "The Group Member Data Updation Failed." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The Group Member Data given by you is totally empty.." });
            }
        }

        private async Task<bool> SendInvitationMail(string Email, UserDetail user)
        {
            try
            {
                var claims = new[]{
                                        new Claim("Email",Email),
                                         new Claim("TokenFor","Invitation")
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
                to.Add(Email);
                bcc.Add("jaydeep.sahu@radixweb.com");
                cc.Add("prashansa.khandelwal@radixweb.com");
                string subject = "Rx-Splitter! Invitation Link";
                string body = "<!doctype html> <html lang=\"en-US\"> <head> <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" /> <title>Reset Password Email Template</title> <meta name=\"description\" content=\"Reset Password Email Template.\"> <style type=\"text/css\"> a:hover {text-decoration: underline !important;} </style> </head> <body marginheight=\"0\" topmargin=\"0\" marginwidth=\"0\" style=\"margin: 0px; background-color: #f2f3f8;\" leftmargin=\"0\"> <!--100% body table--> <table cellspacing=\"0\" border=\"0\" cellpadding=\"0\" width=\"100%\" bgcolor=\"#f2f3f8\" style=\"@import url(https://fonts.googleapis.com/css?family=Rubik:300,400,500,700|Open+Sans:300,400,600,700); font-family: 'Open Sans', sans-serif;\"> <tr> <td> <table style=\"background-color: #f2f3f8; max-width:670px;  margin:0 auto;\" width=\"100%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <a href=\"https://radixattendance.azurewebsites.net/\" title=\"logo\" target=\"_blank\"> <img width=\"60\" src=\"https://i.ibb.co/hL4XZp2/android-chrome-192x192.png\" title=\"logo\" alt=\"logo\"> </a> </td> </tr> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td> <table width=\"95%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:670px;background:#fff; border-radius:3px; text-align:center;-webkit-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);-moz-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);box-shadow:0 6px 18px 0 rgba(0,0,0,.06);\"> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> <tr> <td style=\"padding:0 35px;\"> <h1 style=\"color:#1e1e2d; font-weight:500; margin:0;font-size:32px;font-family:'Rubik',sans-serif;\">You have requested to reset your password</h1> <span style=\"display:inline-block; vertical-align:middle; margin:29px 0 26px; border-bottom:1px solid #cecece; width:100px;\"></span> <p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\">Hey Hello,</br> "+user.Name+"("+user.Email+")"+" has invited you to . A unique link to reset your password has been generated for you. To reset your password, click the following link and follow the instructions. </p> <a href=\"https://radixattendance.azurewebsites.net/#/" + tokenString + "\"+ tokenString + \"'\" style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Join Rx-Splitter</a> </td> </tr> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> </table> </td> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <p style=\"font-size:14px; color:rgba(69, 80, 86, 0.7411764705882353); line-height:18px; margin:0 0 0;\">&copy; <strong>https://radixattendance.azurewebsites.net</strong></p> </td> </tr> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> </table> </td> </tr> </table> <!--/100% body table--> </body> </html>";
                //string body = "<a href='https://radixattendance.azurewebsites.net/#/ResetPassword/'"+ tokenString + "'><input type='button' value='Reset Password' /></a>";
                string from = "svni7071@gmail.com";
                string displayName = "Join Group";
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
