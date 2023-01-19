﻿using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Repository_Layer.IRepository;
using Repository_Layer.Repository;
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
    [EnableCors("CorsPolicy")]
    [ApiController]
    [Authorize]
    public class GroupMemberController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMailService _mail;
        private readonly IExpenseService _expenseService;
        public GroupMemberController(IUnitOfWork unitOfWork, IMapper mapper, IMailService mail, ISprocRepository sprocRepo)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mail = mail;
        }

        [HttpGet("GetAllMembersByGroupId/{GroupId}")]
        public IActionResult GetAllMembersByGroupId(int GroupId)
        {

            var obj = _unitOfWork.GroupMember.GetAllMembersByGroupId(GroupId);
            return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj.Result });
        }

        [HttpPost("CreateGroupMember/{GroupId}")]
        public async Task<IActionResult> CreateGroupMember(int GroupId, [FromBody] JsonElement lstMember)
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
                    bool res = false, result = false;
                    var mappedMember = new GroupMember();
                    foreach (var member in lstGroupMember)
                    {
                        member.GroupId = GroupId;
                        mappedMember = _mapper.Map<GroupMember>(member);
                        var existUser = _unitOfWork.User.GetT(x => x.Email == mappedMember.Email);
                        var existUserInGroup = _unitOfWork.GroupMember.GetT(x => x.Email == mappedMember.Email && x.GroupId == GroupId);

                        if (existUser != null)
                        {
                            mappedMember.UserId = existUser.Id;
                        }
                        if (existUserInGroup == null)
                        {
                            res = _unitOfWork.GroupMember.Insert(mappedMember);
                            if (res)
                            {
                                _unitOfWork.Save();
                                Summary summary = new Summary();
                                summary.ParticipantId = mappedMember.Id;
                                summary.RemainingAmount = 0;
                                summary.GroupId = mappedMember.GroupId;
                                summary.IsActive = true;
                                summary.IsDelete = false;
                                var summaryMembers = _unitOfWork.Expense.AddInitialSummary(summary);
                                await SendInvitationMail(mappedMember.Email, GroupId, (UserDetail)_unitOfWork.User.GetByExpression(x => x.Id == UserId).Result.FirstOrDefault());
                                result = await _unitOfWork.MemberInvitation.InsertInvitationDetail(mappedMember.Id, GroupId, UserId);
                            }
                        }
                        else
                        {
                            existUserInGroup.IsActive = true;
                            existUserInGroup.IsDeleted = false;

                            _unitOfWork.GroupMember.Update(existUserInGroup);
                            _unitOfWork.Save();
                            _unitOfWork.Expense.UpdateSummary(existUserInGroup.Id, GroupId);

                            if (!_unitOfWork.MemberInvitation.IsMemberInvitationExist(existUserInGroup.Id, GroupId))
                            {
                                await SendInvitationMail(existUserInGroup.Email, GroupId, (UserDetail)_unitOfWork.User.GetByExpression(x => x.Id == UserId).Result.FirstOrDefault());
                                result = await _unitOfWork.MemberInvitation.InsertInvitationDetail(existUserInGroup.Id, GroupId, UserId);
                            }
                        }
                    }
                    if (result)
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "Member Added Successfully." });
                    }
                    else
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "Member Not dded in the Group." });
                    }
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "The User Data given by you is totally empty.." });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "Member Data Not Found." });
            }
        }
        [HttpPut(nameof(UpdateGroupMember))]
        public IActionResult UpdateGroupMember(GroupMemberDTO groupMember)
        {
            if (groupMember != null)
            {

                groupMember.UpdatedOn = DateTime.UtcNow;
                //_unitOfWork.GroupMember.Update(groupMember);
                bool res = _unitOfWork.GroupMember.Update(_mapper.Map<GroupMember>(groupMember));
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

        private async Task<bool> SendInvitationMail(string Email, int GroupId, UserDetail user)
        {
            try
            {
                var claims = new[]{
                                        new Claim("Email",Email),
                                        new Claim("GroupId",GroupId.ToString()),
                                         new Claim("TokenFor","Invitation")
                                        };
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
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
                string body = "<!doctype html> <html lang=\"en-US\"> <head> <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" /> <title>Reset Password Email Template</title> <meta name=\"description\" content=\"Reset Password Email Template.\"> <style type=\"text/css\"> a:hover {text-decoration: underline !important;} </style> </head> <body marginheight=\"0\" topmargin=\"0\" marginwidth=\"0\" style=\"margin: 0px; background-color: #f2f3f8;\" leftmargin=\"0\"> <!--100% body table--> <table cellspacing=\"0\" border=\"0\" cellpadding=\"0\" width=\"100%\" bgcolor=\"#f2f3f8\" style=\"@import url(https://fonts.googleapis.com/css?family=Rubik:300,400,500,700|Open+Sans:300,400,600,700); font-family: 'Open Sans', sans-serif;\"> <tr> <td> <table style=\"background-color: #f2f3f8; max-width:670px;  margin:0 auto;\" width=\"100%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <a href=\"https://rxsplitapp.azurewebsites.net/\" title=\"logo\" target=\"_blank\"> <img width=\"60\" src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGwAAABtCAYAAABEOoRoAAAACXBIWXMAAAsTAAALEwEAmpwYAAAgAElEQVR42u2dB1wUV/f3R3ZndpelF8WuMfaosXelgxTpIkWp0kFsKNIERVFU7L2LBUSx99h777HEXmOa0WhM0d977p0FTZ6YxLyPT/I8f/fj8c4uM7Oz97vnnN+5c2dWEP6HHlK3uV1VcTdGC+8f/yXAnGZfrRB6HgbpL6Lf98Y//CF6rBiu57YCFby3QhgCfDjwlOv7XvmHPgzd5zVXeKwGM6XLElRI+hbCYKBtzpnm73vnH/ao6D6hgtJz9d0KnutBLZRO86EMPQMhDVAPevkkdPVXVu976R/yqNJ5gEJtM2Wh4LqCYK2C0msN9zCl3zYohwJCP2Yvrtuv/0n5vrf+KeFw6LkivaR7BGk3lN1XQulB5k4Ak59AmUnAEgCjwU/3vu+pf8CjVuFPXpqsn24pbAqhbJ5BsMjLvNdC6bwAyoiLHJgilaDFApWHP1v6vsf+xofZKkQIMwhG6F0o2+dB2WY4hcVSOY+5LIbSfxeUlMeUBExBqlGIA1qNfDjqfc/9HWHQY4G3XtRhKBKPQxxwEWLmF+RRpwjUMtnLWEhkobH/MyjTCRopRr0UBheI2Yz3Ndp/8qHuvtBCcF/zQs92IRS286B0WEiAiklwlOo8TGfOpBajPoMiiwF7CWXKSy5CVNlAyohxefmDe2WnpQ3NSxk0MDkrM90rZ1hWs5G5I9Tve/jf/ND3WHKogscaKLqXUN21UvYoJjbKBAcLiUwxUh4Tgw6We5hyEIVG5mWU1+qGLkNyhBcyM4ZeSR82HGmZwzA0PQNDUlORlZX5ef7okbvG5OVmLVww5+P3Pf7/8TBwXRqpF0ihsAfJdt9NZBuh8KPWm2owj1IdtFWyh7kXQaRlsf+PUA6RgSkHvIREeU2gZYvO+WfXb1ylYfsdNX6qVfqIcW2H5RWERSUNnhebPPhq8sAhGDI0Hbm5wzE8O3PFkJSBnd4T+JOPr7/8wubwjjXZbh4+j7RVW0JRPxjKjmOhtJ0Opf0s8qZFsle9HhIZuG6kFqNvy+Jj4EsOTJEMSOR1gvcdNHXJvfCm9wyOHqgIiR3oGJ2ctmjg0OznyQNSMGjgwB/ycrNzZs2aYfmeyq8eACpevnI5b8bk8c+ievqgRQtrVP/ABhqz9lBYOENRxQ/KWgSuZhCUrbPI28jLvHThkHsZC4vzIAYfkz2sP7OXEJMJHEFjEIWPVqChc8buP3M80xYUuQzPn/JJXN9BiItPQFrq4Bnz5s59D45A1T129MjSWTOmoZtdZ/Txd18QuHjnDuX4+9AbBogJtyAG7oLotUoWHK3yoew6FUqftTyHiTyvlXAvE91JNXqtJUgvSE0SpH4vOTRl35fyc4InVBsN+5h5C//s8c1asrpOzrgZyyPj+iM6OhoF+aNT/6+CqnHk8MHFM6dORFhYKNq1ab7YwdHJOKTgqJnl0BsPhNjjUCSdoFx0lULafUiZD0nOk6V/ATFgD4U/qr08V+pC40o5fxE40XURxOjPSSXKgJiJScx0EHtdgGCRBb+8Hdlvc7yb9hxvPDAz/4BfQCgG9ku+nj8mr9X/GVgXP/00Z97MyYiPj4W7T9CCZs1bVS8vkMMOfCl0IE+ypzzFZLztXFomye5Ez92XUoG8BiJ5l0i1FwPEvcxT9jKR4IkuLCwepxwGGVLiT9TqrO8LSInPoOe+CUKdAiQuPN37bY99ysLSiJ5hSYiJjkHOsIy+/9Ognj57Zlu6svjzjLQUBIYn7O3iEd247G/dsg/oWQZvLBFsFsud31025etG9ZfoViS3bB1uOlhkEgGT3AohkaIU++pCYeKPZD+QPZet788Qw69AcCiG8MEMJM480fltP0fe9GU1PXsln4+Ojkd2Zvqs/0lYZ8+cGTexYAwS4mPg2Tsh+Nd/twourir12PRCz3O9nK/YGKHvBvKm9bLRc5GNzjOP4jBlD+NeVdaSp0kMXPdlkKIfQKLCWeKgvoeU8IyWvydPI4v7BlLPfRC6LIPGdf1PC3deqf1XPlNb55AV0TF90S8xtvR/KVd9sH7d2rNZ6amIjkvc0sHR6zeVVrWwHY76vc98qeg6G2KrdCjbjYDYeRxE68kQ7ek154XkXcsIVLEMzKPkF172Chgtu8yH1OskVKQUpYSn3EQKhWLCE7LvaJmeh30GyW8zhE4rydtK7o/d8kD/r3y+1k4h8yKjEpCaMnDlf38IfPrUfd6cWRg6JAVBEQlD37RezXzYa7yOvRQa5kBZIwRilQAoq/pDrBYAsVYviHUjoWyUBGXjfgRwCkSS9BzS68A8VnBgfNl1MaQe2yFR+JOYRzFIDFb8Y0hkYvy3sgcGHOLhU7DfAMF1x9G/+jmbO4SuiolJRHrqoPH/tbDu3bufPG7MSFa/wMM/zO1N65kHLrQXcp6gwqAvIA24TDnmIoWsMxBD90P030qdT0C6ziE5Pw7KeoNomTzOb4MMpnvxa2FxZbmnSRQupe5FMpS+P5GHMVAEKe4RwXpELYXE2K/Iyy6Tl+0g+4RDq9l7Q8lf/bxd3cOuJSUkIXfECM//OljXrl0fMWJYOrKysp7auQY0eSOsLgOsFNYFULhRuAteTx15mKBdgJh6jaT8XUhZX0LKoQ4e8QRSLrWD71MeI1huS8u9i+csTx2ocqPQ6LIQUsg5SCTrpbivCdg3Omjf0BfiK25SFL1H4BFIPvTFIHCC43Y0iNlW8Fc+c7UWbrU8gxIxZGC/F+PG5Rv8F8G6lp+VPgS5eeM+d+6ZXO331pVsRh+rwE4+dlvAc5TSgZbtSJbbzoFoR+ZEuch9CXkAAQjbTF7BQhgDtpyLDolqLxlYSXn+4rCYuRIwf/KepOcyKOZZDBqDFfsFNynmIe33EoXP3VD12ClDc9iChoMv/CW53qBLcEZswgBkpA0p/u8Ig/fvpw3LSEVOzvBvHIKHVn7Tep38BilM2iUWCF1IVLgtps4v0okJnXnoFCB/Tsuu5FHOhQRwsfwaz1XFHJZYBuk1b5OfF8ttDBXdiU84KCmOPDaOQDFYsQQrhkJmn1uQgo8RtJ0EeBf0fCg8Om2ES9be7n+lDxz94r/ISktFSsqgj/7RsL777rvAkcOzMTJ3xEsH7/A6f7S+1mf5AYV7KcRu82RoDIL3ahIU62RRwYpjHyqSvUq5zOdWBpLaX0B6HZSnXI+p2LLbIqjCzlMe+57DUhEsla6VYj+nHEcwo+6Ql33KZb7kT55G0Cp4EjTX/fAce7rZ2/bDh516RfRLycTI4Zmb/8nSvdGUyRMxIicLvr0Tu/7R+nrR1xpXiH50VrQnmf5REsSmySTlMyG2G8mlvGRDStCRpLzLIojuFP5YyPMulVsOjYXDFTIcDuy1Zc+ScnAq1wVQ9SQISU9/AUtF3qWK+ZybFH0PUiTlzF4nCNgebqqeOyF4EDSvvY/9Zt4ze5u+qN42WM+jV7/vp0zIR0ZGWqV/JLC1a9fczUxLQeKgrCF/tK7RJLRVDMFzPfd9UDbLgvhhAkn3KIjVSc5XC6I2EGJtWq4XTTK+L8SPUwlmNqQ2OZAop3GvK4P2OqTXwem8TUUFtIo8VU25Sk3Cg4MiU3MjYLEPZGBRFBZDL+i8TAeN8p/gvhtq/73n37Y/mtmFzRs7diIKxo355w1dXb58eX7akIHIyR2974/WdUhe0EIIOQm9RPpGD6BOGkLf8NTPIQ66RXXSeYgRRyH2/IRyFnV4F1ZAU45rTFDrDIBUOwYSeZ3kt14HZqWuLQNXBksHzIuAeZGXdS+ksHgBaspjDNIre0DA7uuA3YYU8RlUlMu4RwbshTpgNzSBuyB47oVp+NFNb9MnH3QKdRmYPgZzZkw++k8Lhe3z80Zh5MhcOPmE/eG5Ik3H9C/1WqRC6ppL+YVUYOAaSvx7Cd5ZAncVUgZ1Xg5J8Fwm4ymMsTabhEIaye8+5yjX6eorLiheB/RLUzHz0i1TyaAK2g8V1WIqEiAMkkoHSx17D5q4e9CPuwP96GvQRp6DQcgBGPTeA8Pee2HUexeMQvdC8N6D+v0OTf2z/dLQJlTdp382ipfOx/CcDOkfA2zzpk3X2ZBT30EZmX+4ctOkuULHPEj2BRAdppLim0UteZEdtdYzZbMlcyQR0p2kfM/VkMK3UtG7n+ovqqf6XyEAq3lOk8pzWJnIoGWvVa9yl86Yl/GygEoBpggZKDUDFH8fBgn3YJR0D8Z9yZJuwzjxBoziLsMo4jiMQvbDkFnwLrKdBPEghJ4nUCly16A/2zfdQ/qdLilejLmzJrf+h6jCp72GD8vEiNyRX//RumLzKA9le8pFduNJns/kY338vJX7Un7SUbbl3HO4jGdCw5Vec15CUKmotltA4ZBAehb/KvyVQVtRDup10VH2N5XHUmj6XIS271cwTLxLoO7ChECZJN8nI2CJd2AUfwvamOvQDz8LTfBBaCgsagL2QN1zF7W76Qt0AMqgE2gUs83nz/RPJ8/4wrnz5mL+3Gkh/4RQqLe6tPTz7Mw0xPVL/8N5f4bBmzJEv10QWd3VZTQkh+kEhECQKBDJC7iQ8GW2lkt7iUv5lb9ShSWv8lY5sOJf5a7Xva4EagqLGt+V0HougGH4QRj1/4a86S43w4Q7MIy/AwMKh9q4uxQSb0MTdQOaiMtQ9z4BNYVRTfABaHi7D/q9D0NF+VcZfBQWwRvb/vH4Yp+JU6bPx8xpE4f87cCeP//ePTdnGPOup3+0rt0iNFQnfXdBaU/e8wGpwNo+EBtEQGraF1JLymdtSf11zqdwyMLkXPI+NipPnsU634fgMZh+6+SQ51H8KyW44jXh8ep1ldcKaHxKoO1RAkP/Ehj5FsIoaAMMEu8THMpXMQSHWfQtauk5hUkthUlucbeh3+cSNKEnoQk5Cn1mvQ5QexgG4SegH3EOqshLP1iG7/ndUZwWHoPyZs4vxtLFC4b/7cB27dq1PDtjKFLSh//uuFuV4V82VY/F0wrhpMbs5xKckZCaUb1VLxFSrXBI1QIhVe0Bqbo/qcBgSHUJZCOS+c0GynVZe8p5XQqoLptKuWiZLk+97k3/KjY0PuRRvitg2GOFDIvM0G8F9L2WQxN+UVaIHNRtWXDEfw5twkNok76AQdJDym0ELfYGtBQa9Xsf5Z6lJQ/T9joIbcgRaEOPwiDsBAwjTt2yCt2qetNnt+41PG/Vpj0oXLIk++8Oh9KUyZN+yqb81SMkvuUbw2DHwZZKr+U/CL23QxV9Cqp+d6Aa/BVUaY+o/VyW9QmXSE4fgxSwA5ILqb8uM8nrxhC0dILXj7wxFlLNSEj14yC5sjPJa1/LYa+Aqei52rsE+gSq3Kv8GTR6zYf+RuFV7bYY6l5HoEn8hkDdhSb+AfQTPuemTSRgCfc5LMN4CpcEzCDyPLS9CRCFRG3QHmgDd/PWgAAaEjRN2BlUDNl28E2f3yMmf8qug6cxd978AX8rsG+//dZ21PBsDM/Nu/d76yk7jzio12owecogiB3IW+zGkZeQlwVQKIvcSTL7OFQpV6DKIPU2giDmkYwf+R1Uw59AlfUNVKn0en+S+tHHCRDVXq5LXnnWr2S92nsFh2Wg8ypD/5UwoOcab1neM2Aaj2VUV+0gYF9BQx7FYVEI1CdAWmaxt7kZxNyAIUl8QwqLhqEnYEB5zIBgGVBNxlvyNsPeB2FMytEg7CT0Q08s+63PHzZkVtHJ0+cxd85c/78V2NGjRyfmjcjG0KyRb5wqpt8pdZCy80iS6mMoN5HQsJ8kS3mHaRQayezIbKfJoc6OvMp5LnnJEipaS6nI3UIwDxJMKnazb0NVQDD7X4fKZSmtU/xKsutaGVYJD4FGOlgsJGq8yv5OsEjQ6PuR+a+HJpbCYeKXlLfu8vqLg4ohhRh9k4w8K/o6DKOuwijqCowjzhKYIyTtCVLgTjKCRl5m1GsfB2YSegRGYSRCem8b+es+yJlUcvHW9WuYOnnq3zsIvLJkxfkR2ZmIShj8m66uaptUS+w8nEDpJHw3qrW6zaGQN1c+de+6QD71wSbLkKzn1n25nKPIi1TdFkPlOB8qO4JoOwcqB1ruTrC8mZggYzKdhIWa5SvyIK1fWb5aCSNqtQRPTeuo2TrepdD4rYXGfyP0e26g8EghMvwM9BmwWBmSPilDLTOCpY1hwHQeFkU1WeQFmIQdh3HvAyRadpNRXUawjHrthwkV2KZhh2AafgT6YadQtdeaiLI+aOcxyGjl1uO4e+PKy/zR+Xp/Z/4ymjNr1ousjDR4BkQ5/9Y6VrH7u1It86OifS7Ej+MhtksnKU/iwW4iedIcGRATD6T+VL7reF5SUcfKMIplI09Ss5bUnpp71Qou0cuMPeewfGVQxj1LyFaSuCBv8tIZg0WepemxnjyLYLHWuxj65Cn6cRQKo3WQyu0mhUMyBouMeZhRn0/Jy84QtKMwJY8y7bUHJr33ke2HaehBWEQchkXkEZhFHIFJOOW10GN2vGgOG+Z9/MJNHDl4aN/fLTiaTJ08CRkZGXDwDG30L6EwB3U0fR5c1+tQANGyHUTDehDNW0KqbA2xpiukegGQmsRARXlN6kByvutY8qQpUDnPkwdqGTTqZJXfeqh6bICaOllNz7nHeMniQcPrK10Y5LBkM/JfRR5USmpwJbW03GMdtD03ykbL7Dl/nfanjbpG0v6WDC1GDoOyZ12XvSuaQmL0ZzCOugiTyHPkRcdgRt5kEbYf5qH7afkgzMIPwTLyKCrGnIRl9Ala7zhM4j972SBhh+mAvMLsLx5+jeKSHUl/N7BOUyZNJGCZ6OToW+X1vwUtu1pPGIxvhYgH5EUkChwoHHamsMjGDhtEkdrrAdHKmQB2gWTeAZJFJ0hWdiTpCWSdnqQMaZ2P+0NqkwFVp1FQWRdA7TADapcFOmAyqLJQaEAK0IhAmTBgAaXcu7QUNrUETUv5SkteZaADZkC5y4CAGfithtarCAZUY2lZ7UW5yoDMsMyrOLAbMI65CuPYqzCJ+QymURdgFnWavOkILCMOoWKfI6gYdQTmfY7CMuY0rOLPwirhLMyjj8FywANUS//y5oodZ+88f/QMc5ccr/K3Anv58qX11MkELDMLnW1dfjHYq7af+FDpOItC3BpIFPelhBuQ+lH91f+B3Pa9DSnuMinE4xADP5EHcW1nQGo9nIBSEV0rjGoyP/JGD6iquENV3QuqGj5QNesny3LvVXJoJGD63kwR6kJhwCoYk3cZkPdwYL6rCRDBCSBQAZtgSLmLP++xlnIdmXcRiYg9JN/v/wIUt5hrBOoaTOKYXYdp/A2YxV2FedwlWFBpwkBVij6KynEnCdQZVIo7gypJF1A9+VNUjP8UNXN/hs2Ee7h+5QHuPnq54J8wJNVuxtQp5GFZsHFwrVr+h4/TFgvtKGex8cK2Q6ilvNUpRx47dJ0D0Y88juoxKZZADrwGaehDSJkk3TOf6OwRJJLxUvJnVLOdhBREst+dZHtbCpntR8uCw7NEB4zykE8xr7GMeChkhTHzOgqFzAiYlsDIwDZyYIbkYRwWCRAjX7bdJhjH3dDZdd6axF6nkEaQ6ItmlnATpkm3YEZmnnQTFolXYRl/ARVjTxOsU6jW9wJq9b+MmgOukH2GOim38MHQx6g/EZiw+Svgx5coffRiwz8BWN2Z06chPS0dDq49+TRrTYeBPRQ246G0GSvLeNbaUQ6zn0g2mZtEeUq0pWUbEh7WE2VZ70Te2H0RpJ5r5bqsL4Ea8hmkNCqwswnmqGdQjfkRqogTtC7lOI8inaxnw07kYX46Ga9Thkxo6HuVyHmMAGkJFAuLLH8x7zLwY7YGRsxoe6OI0zCm+kv2KB2oxJsEiCD1vQuLfndRsf9dVBpwD5X634JV8lVU7nsJ1cijava7hA8H30SjrM/RMOMLNEz/Dq1mA76lP+PeFy/BHmFHX8Jm0ffL/m5g6sUL5n+fkTYEbp4BXBHpdVt4XuHITpUQEOdZ8sUIJNuVbOoaG5UnSS+SOmRzNyQXJukJknuhLOvZMpv06Uh/syWAtjPlMOlIarI7yfte66g2IwHiRoLEg43mF3EVqSG1Z1Am53sUy3WXp84odOoTHA6NKUOCpWW5y7eUimkKlxSyDbyKuTw3jLstiwvKWSYs/CUyWLdh3u8eKg64D6uBZCmfo8rgz1Ej5S5qD75FoK6j/tBbaJz1EB+P+BYt835Gh8mAx35g/X0Z1uEfX8B+L2Ce9QJO4+7k/q3QNm1Y92lu5hD0CovwY88V8V/eULaiMFitM8QPPSA2iYDYqh+FxGHkaRMgdZvLR+X5uSkf8ibf9VzS8+d8mKlIrsPYOq8bq8tcFsu1GT9FUiwD82DAyMN8i8nDqOP95WWNFzMZGKu9uEr016lDJkIYLJ/VMjS2vf8mudZiBXL0FS4yTCkkci8jaBb9GTTyrkEPCNgXqDnkc9RJvYcGGQ/QNOdLtBrzPdqNfYEuUwH/s8D0HxgsGVju9y/ReR1oveeolHgHwbNv9vnbgJ07e6ZofHZ/JOXO3qoc+OMOPTpihdoKIjmgKFSAqKeGJJlC0taA0rwZpGo2kOr6QGxMoqJFMqSO2eRFMkipDCQ7wei/GVKPjfIyA+q9Sh5+YuuwUMhh6YCR0tP6FFGhXMzNgHIkEyIysJWvAVsvG3tOpmXAKGQygWJARbRR+GkuNIyiL5MivAJTEhhmCddhwYAl34ElQatEXlZ50H1UT7lPHnYX9YbexUfDHqLFiMdom/cSLp8Ag16+xEMdrFPU9rgGDrL1qO/RaNg3ME68h6gFV+z+prD4vMesibkYmJoF08RPoQw9DqULmzc4n5/rEpsnQ2wQDLG6E0Sz5hC1tSFKFSGJ5gSyIj2vSa83ITVI8r62G6T6gZCaxZLASIOqyxioHKYSzHnc67jQYC0HtbwcmNpzuSw8CJQxh0Z5zEeu1fT5qZVSHbANMjC2XOZlZcbV4j6CxYBdIg+7QsJDBsaFRvJtWPa7Q152B1YErBoBq5lyBx8OuY2GabfRJO0rdKC81f0isJTlLfr3Pf3X/+FLWC8G2oz8CS1HPaGw+RVqDiL48ZfRb9lndf/jwJ4BYunK5S8ykoLRIXIqhODDUATsgRh+BhKFF4nqGpG+tVIsCYg+5yGFHCLv2cLPcUldx5GXpZDHEdDKjpBMW0MyaEQQ60I0qEfPm0JViWq06o5QsUK79WA+66ncuzyLy1uWx7S+OmD+sgjhXqYbBdEnb9Iv8zAObQ2HJgMrlfNYz80UEi/zAtk45hIBY172GUG7CoukG7BMvoWKyTdRqd8NVBlwGzUGyTmsLinDxjlP0XYLkHjoRzx58oKHw/x7QPt5QMvhz9GMwmbjDPLIIddRbzB9EeJpn+FHPh9S/KnhfxRYm7QTqYs2HHm5akY6whIzIdhOh6JjDpRtSdJ3zpEHe7vNprBGoa73HkjRBJLNkmK12KCvSQmShE/5ip7forqMgIYelGdCOdE27UdA+oidK6NC2rAFeV4ciY6tckjkHrfilZcxb/KWw2F5aPTV5TFP9jdZMWp/4WlrZcXI89kqXkgbhp2AUR8CFvUpTKIvErTLlMtkaOZJ1zkwq37XUXXADVQfcBXVE8+hZtJt1CcwDiXf4cSdpxzWvJvAxzOBRmmPCNJN7oX1Bl9DrX4XSFmeQ7XE09DQexnEnDn9H4OlatPPRmg+GTkbv8Pp/esxKC4Y9fwmQKCQqOg6Rh7wJaEh2k2iZWptSN5bk1dZj9flLYLiRTkpaBN5H4FKOM/Pi6mGEsCs77ip0r6Rp7/FnCNIDNQSWZSUyfpyef9aLtOFRmMOTR6pZ8brMt0oPR/54O06nacxL1tOYXEvDCM/JTtH0C7ANOYizAiaOUGzTLqGSsnXSdJfg1XSZVjGnIAlrVN9AgmKWY+w8ji5FF6ghHJWsylADSoFahOgWv3Oo0ZfApt8jtqzqBx/gmq4Y1RcH4de0CHUS9y3KXbhVfN3Dqx6YGGBXrcVqBuzAztP38WKGRnwD0tCBbcifpcahfNcnYyfJY/Us5lRTFwwec8kPTM2Ndt5njzH0J4kvN10KgmopTJAos6XgjZAFbUfquRLVKNt5dswSFxNeiwrByZ5/goa5TFjCo3GPYpeQeOe9qqYlj1tA4dmQCHSgPKeYeB28rALVJedgXGfczCNvkDALhGwy7CIv8xb8zgCGXUSZhHnYDWaitHJ32LmFkpe+BHrCNZHBLASRZFKUQdRNeEEqiWcRCUCVCn2OKzijsKKQFWKOwGLmGMwjzkORchJ1E3a3/Sdwuo47Jx55YQzpxSNwiEIIgYMycanZw4jY2AMWnYfAKEZSfl2qVBa53NIIokEPlfeZ50s59lUa48iWeK7L4HIajE2Ba28XUrQlshSntVl9rPkWo171XKddy1/VY95FJd7nJrykZZEyOueZuhbRAKkWIZG8LRs6Ip5FhtXZCMgzAicERXYxlFnSdpTSOxzluwcTKJemSn7W8RxmEZegPnwF/hg/CNMKz1NsJ5h4w2gwZgXMCWPrBi5DxaR+1Ep+jAqEyRLai3IWGsZc5Q87DjZCUhhp1E7Yf+7vbqlcv+rRkLy88+EzlOhZ0hyncSiimxd0Twc2FqCuOhIVK7RgIMUNeYQzRtDqkFyvh7J+aZ9KDelkScVcE/jUp0giv7bSIyQB/lt5LUZmztffjb5FzXZcl1ILAuHy16T+EWv5bTi15RjEUz8deDIi7TsVAyDxgaNWShkw1QEzChwM4x7riMgx2BEnW5EwIwiz/BRENlOwjD0COW2S9Af+gL1xj7Bwg0XKAo+wprrQL1RP8OAQqll2A5YROwlYAzaXlSKOUKgjsA88iDM+hwhOwyzqMP0PmxOyCE4p28xfafApN6HLlSIugGp12Eo/XdC6bMJgt18NO+/G1cefImSOaPQJ34gTBu4QFBVg4JgKnld9pqpzUgFkhKs0h5SHZLzTaguazOIlONISA5T5JOarCbrsQmqwJ1yLcI6Fi8AACAASURBVMY8jMNazsOhHBbLnsuwJM/icm9j44xsNJ55lwmFR9OeshnzmVMlBG9VubEhKuZhbJTfsNduGJLgMCBghswImkH4KWhJJBj3vQ/FgB/RavRDHDhyFfjpG8w4Q/lq2I98ZpVZyDaYh+2CecQ+mIbvg1n4XphTfrYgQKYR+2Eathcmobth0nsnxIC9aNVvx7B3CsvALneh0GU8hboxUFJ+Yre9U/TYDjHkKIRe5+G39BEe3L6MwgmpiBo6DQZRxyFQpyvtJkPJTl42iYRY05lqr8YEjeowQYJEAGXTo9rMGCJ5rWRBRXYNW0gN/KFqGgkVCRg+za1sFISFQ10rlddkOqnvSWGRjEv6MvXowzyshICVwCyALHAVLa+i11YSPFKIVJhzr/Ms4gW1lupJg/CTHBKb3qalvKYd9BzKhK/Qo+AC7l08hx+/f4hBOwCzwd9DQ19eIxJPDIRZ2G6Yhu6CMS0zMwmh10J2waTXDvLirTAK2AxtwA4qIzZ99U5h1fIpcBdsZkFhPxVKewJAYU1pQzmq6yjodSAJ32UkhK7zEVv4BR7euohlEwcjvmAtzDK+gRD3NcS4m1DG3yVZf4fqsssUv4/ymozlLbHLKH5GWvrAE6JFK6rFakFSmBBEUYbZnP4WuOtVaGRwysCVwyJQ7DkHVvxKHXrJsp6JCiN2GsZfBmcawGwFTGjZiI1FUhnAh6k8l0E/YCfUwYf4BFLDuFvQi38Mq9jTGD13O17ePoFzNx+CBCvU8V9TUf4JhVbyVBIwJoEEjYVWdiqHnxXYQHUhvc7OCNBrLEca9aR8GbwDVQKLfN4ZrO795tSU7Kc8r+CyHEqPFVCya7VI9SkdpungTYKeDZPwEyk8Lkbcki/x4M5VbJwxBClDR6FWSCkE5xX8ojoFdaIYRGEu8iQkKiCl5AdUk5F8T/kGYn+S8AlUp0WeIoW4nYCQ6GjRj0qBsXwmsNR9KQfGz0h7lNny8pEPBorlLzWFQnk8sVgetWfq0EsuprUMCstvvit0arKY5zhTXWvktZgrRyMqnDXJj0ilXoVr2jbsXr8cP39+EbP3P0PjsYCy903yyDXQeCwh72SeuhbG7JRND9kM+dmAUtr/OvLojTDvtQnmwevJ07bAsteak+/Uu1oN2tpRcN1IBfFgKFv3p3oqXx51J0VXfrsgqpP02JAUvS7YL0Xw7Ae4fvsejq0uQF7eGHQZuBmC11YIrkUEeAoUtpOgJBPtp8pjifTNFulbKYXuIw88R8X0barDvoWK8gYXHwSb5y42Caf7L2Gpdd6l1oVDtVfZAHDZctkIPi3r8pvGU1aOWu8iPjRl4L2MQC6DSdB6gnUW6thraJt6EFPnLsWjC2tx5rP7CFn6EyzTfoYmiGQ/gTf0KoQxeaYJea5pz1IKt2sIzlremgasouXVMAteC/Pe62EZwqCRCg3aiGqBC5u8M1hhm38WrRIv765QL7RcQHDTmkNp2RjKGl0h1veG2CIeyo5ZUFBxrKK6S+i2Fl0mPsO+O8Ct83tRPHccemcsRpVIdp3VNrlW67aAm5LqK16b0TIf5XCYLk+B4+fJlsjz5vmMqqW/8C45BC7n44myFemmDui8zVMed5TX0+U35okMGhstYRdeuMkztfQDtkLsdRR6PXaiScxq5MxYh8vHt+LOlePIXXUTTUc8gjb6KoHYQbCWEKjFMOuxnDyzCGbkmeaUFy2CVlNbykGx6QomAeRhBM+kJwEM3EBqdAt5cem7Oy8Wuw1qkxycENgVkqTklFYtoDSozIEpXjPla0pQKWmgoHUkq48h1PREFe/pmLr6NK5cOIGLh9Yjd/R4OA8qhWnIbgL3CRQ+G6EgD1W4LYOSlCCvydwW6+oxnZXL+qUyrLLWU+dhfLmoXHC8GiBeztdj24m8XcLVJp+v786my63hFzkIPQ5A4b0JreJWI33iShw/sBOfHdmICaOz0bb3JCoRNkDtTiGUAOt3mwUD9zkw9FgEQ89C7mVG3ssJIIVXUplGTGn6sTMAbE5J2dwSVvexuSQbUNFvsck7A2bsW3RIcN8BsfdhiOHnoIw4T9/Cw7xu4pcHsbPKLROhrOMKZcUmUKoMyuHp6doKZGxg39vTC2tWrcCdy0dxavcqpI6YBIeBpbAKXEkhlMKoK+VHgqfssZVsG5R+W+j5OigZMH6ic4nc4WVexkVHmbe9Alm2Dmt5yGZGkBSuhbS8XC7cA+jL0vMAD9GVe62FV+pKTJy5BGcOfYLj+zaDneNr074L1OZ1oWfVCdpWfaHumAt11zHQ2BdA4zgVmm6zoXGdD323hdDvXihP6GE5kudNXQjmU+zk6Qpq3w0w81ue9s5gaezyM4W246DsksdHLJT2U3joUtK3Uhm4myAehzKcAEZeghhxAWLYGYiBe+Q7ADjNgNghA4pGwdCr2plqrjq8kDbQ6COhTwjWF83F7ZPrcW7HIuSPzIF3775o5hQN81a9IdT1h9AoHHpt0ijXTYSSOlrhtlzu8DJzkyEoyRPllr0uLyu4LYGChTs255GKcZHqRcF/HwTf3fzLUDNsPRwHFGPElGXYXLoE54/uxKZtexCdnIHGjT6CnlIDoZYL9NsNgvpDN2joc+i37gdNp3RorEcRtAnQOE2DmkK/xmUe9N1JqFAONiCP17LTPXxUpczYXP/VzMu+eGewOiQWNa9gMxUV7GdC6ThdVoNcEU6VO5HVYSTFlV1Hk7QfJ6/D7gDKbpocROEz5ARB/BRSnyvUXoJe8FEoeu7i326h/WhYuI5F1LhPsGjjSZw6fhzXDm3A9sUFGJUSCy83V3TsZI2GTdrArEpDntuE0E8h+O2hDqeO9/0EFfy2Q893G/T8tqKCz2ZU8CUxw2455LdLNh+23i76+3aqfbbggz6b0bXfGoSkL8Kk6Quxe2MR9m4uwY59R5BXeBA2cXNg2SYMglSJzBTqWjZQtR9KYXAp9LvmQv/jKGhaJ0LdgV6j52rbsVA7TIKaoDFPY16mZV7mSeuzsoDVgWz+PsuzLHfSl9zUc77HOwNm5j37tOBUJN+a1XstCYQVULgshNJ5NsGZ8QogN11dZjtB9sSu5JFUkympPuOvM0HBvDKAOpBdBBd9jQrQ2wThPNR9r8N2MZB5GFh0/nvsunQbZ86fxJE9W1C6aDZy04egT8o4+KUug9PAEnTouwYtEjbgo7jNaBi3FQ3ituGjhE/QNHEbmsVvRKv4NbAdsBq+6asQM2I5ssbOw4wZM7Ft3XKc2rcR27ZsROH6g8hcdBLWqTthFVRCX6AcyrUeUFBIlyzqQyRY6ubR0NiMlGcMMy/pnAl1+8FQd8qC2joPajsCRl6mdpxCHjYH+iw0dl8MfQrN+gyUpyx0mLFr1Mj73t0F6f4TDhiKfvu+rdAsCcraziQ2QqFsT+HJlgplkuzck9idq9lNkUnK8xDVbR6BnPkKHm8phNpP4s9ZfcZOt7DZVAqbAujZToae01zaB+WvniT3Qw/BMPUG2s55hsjNL5C59ztMPPQ5Vh29iT179uLwzvXkFcXYvrYQW1YuwNrlc7B6+TysKVqA9SWLsXXNMuyivx/YvgZH92zC2aN7cezoEazbeRxTSPAMWXgOvvmn0KjfQRj22kEKtghCp7HQa57AFa6yhjXEqu2g+tAZ6o/DKV+lUZ6ayMWMmg2PkdBQWY+EirxLZTMaKoKlIg9TUS5Tu8x+BYy8TEPQ1DynvjorbuCxsOE7gXUEUDTOfXRYqJPI1Z+ezsoVocYcCvMGUNa0hbJxMK/JlJ1HcDjsNnfsstbyu1uzPEP1FQfpSJ7I8prTbN2sKXn2lJ7jbFSwm05F9zSqx+ZA4biIxAcV2QF7YJh8HvWG3YJ13k14TryJwKlXETrtU8TNvoCBCy5g0PxzSFlwDgPnneHP42efR8TMK/CdcBn2uRfQIu0cqsYdg7o3iQuPjRAcl0JgV3XaUghvNxSKj2OhbBgAqW53qOq6QVXfE+qmIVC3HQC1zSioCZLabT5UlAPZvH+VM5UaduOp3CBzmAiV01T+mpo8TOO2ABr3RRRCF1O5UEi2RFanbM5J98I57wRW8bG7eoLXnaNC6EM5mXfIhLJZHygIjsK45i9k/L+YqILStBaU1TpCWd+Xb6dslyqfxOw2Xx4dYXcVZXcT5Z5ZJKtMBtlFviGYwnkB9KhlxbfCaYF8X99uiyGQ4BBcKXS5EUjP9RC8t+jyGLMdPJ8JPttI8dHr7pQjXchru9E2ziQ+2HuzME6hW0EhW9Ell4r/FF4zSk3DIH0UCFXjnmQBBCsUqtZ9oeqcJYc7AqFmd89hF2f03CYrUxIZEnmVypFdAzCDPG82rTOPwMrANBzWUm66EuRHA49F+u8EmEP6xjVCg0lUF62HsvcxKCMvU765TirwiqwAA3bLHc1CW8u+JOXdobRoRHWX/htBKhUKKI2qQVm5JZQfsPAaApGSucTOQrM7B7B7RpF8F3uyu6dtlm8Py35Hhak8l0UcHoPI7vImL9NrlEt5wU1/Lyu85ecL+UlTBXkxNx6i5aEzPlWhy3BI7VMhkUSXmkdB9XEkWQRUzcKhat6HYCWRqEiVQ54zKUAKc+y+HnworMdmqKjEUJEHsTPlKudZcph0mcuhMmBqHSx2VlwuL9ioytJ3cwHExC2XAoVaudCzpm8hCQdFVzKWb9goOxMbzDtINCh7H+VSXsnUH5tgEyqDFNl94kkpivTtVTbwhcKqFZT6Zm8ssLnpm0NJ4VUkDxYpT4rthkAi9ckuQ5JBbpILaX5bCDoGBphJd3Y83GTP5C0/u60zdobbiU1AZRcMTpD32TkHUrvBENv0g9QyAVKLWKhaxnOTWsbJntV+CFRdcuT8xGCwa9dYAc+GxNj8yR5b5LmT7DUGkzyLGQfGQqHHEgJWVguyswlLb70TWHuuf1lRaD/lpdCa5Dp9WAWpQIXDDLklGa+g/MSEAhtyUnQZw+drKO1IEdK3mHkc//2ToINUWFNN1ucyl/FiyGmIQQf4GWY+NYCF1496Q1m9C/e4Mni/CVIpQjSuBbFGZ7nz2f06XBfoZg7P1wFcIE81YFMMuulyIrtYkE9FoHxIgkAir5JIsYqk7ESCIbUdCLF1MiRmDBy1qjbM+kPVLgWqTplQ2eZTbppGnjNHBsaKcH5ujuo5dpKVnTFn59vYneHYXXWoVVMr564l8ogLO7tOKUDrPtf+3w7r8r1v9OuEFN0W6o+DnivLHUwIzCRg08mmyeB4O5UDVPLXp5afYlGQ+uOeSPWYguoyhfVYWSEyT2ChlUELJSkfdo5C60XZQk9RCNwBkQ0XsVDVMkkeKWFiRtSUg2Qtn/PBgLGWA5qrg/SaRzHPdiJznMYvx5XYxB+bMeBXfhIsiV+UMYg8OIVDk1jLlwfJz1mY7JjBFSD3SNqXioph2bt0HsbkOfMwCo384kOmAOlvHJz7EjkMuhdyocFgaTyW7H4n3hU/frVWaDO2j8J+chqFkaVUYx2hIvg+g8TAcSvzNofpOmAE0H6azvum8mK6HCqX8lN0xXW+XFx3ziWYI2WvZPKf12S7OUgxikJr3G3ZIi9A7EWeyu4ySp0nUugq9yAW8liodJ6tC3kzdTZDd700u8CigEDlk1eNIlDZOs9K41fRcCjMy6jwldqXme71jumQuowgjxxL6o+AM2HBgPGQuEg3rlkoh0V2/o613qtlaLrhsld5S56+YOQxt7bwn3po7MeKVGPUU9lP9JAcJg9VOkyZQ17zCUG5JXtemc2QwTGQDJzDFLllIVR3joy3fHmibKyWI4/kIyXc8mVRwItryoMBJD7CKJzGE0D/7XwbDkaXl8q9iUOaLINiV8YwT7UeTcJihDwfks3l75gpW4d07kESb3XWIU1+jf4udc6mPDeKeybPe+wL4jpfd+21DhYLyWy6OJt3wqaP+5bd3GX5q/FMdicfNurffeF04Z/wUNlPqiA6TKwr2k9wFe0KEqijJxKkbQTzVnmuK2+nyIKlHJgMkENjHkgdrWCAyTPFspGTsvmLDCIz9jenmXxkhYFiw19l3qRkkHTFOA9/zKs6j+BhUMm8q+OwV17WMUsOjQRRKgcpv8bh0rY835Fcl/iF83PlHMnDYWF5WOSzvFhI9N0o3yOETVngE4WKXp9f8oO++2KV8E9+qO3zKxDEGqLdpE6UAyIJxgQCs4XsotJ+0lMF5RUF97YpOmiTXoFk+Y7Bcpz2apmBctB5LAPE15ui22aiLvSNk689Y6KCTS9g9RXLWQwAeYycv3JkaPy14dykzmWv6dahUM3DKNsv+0LwuxzMle99xYzDWsKh8WUWrhk0dt01EyBepa/dL4TPKYkT/psfFGbMJbvx7UW78ZEEMF+0m7CCctkJWn4iw5v8KoSW5b/yoS1dKGXDW+yiQDZCwYzPIxnNYbHxStbpSgaMdT73slwZBFvu8prpPJAts9EZ7pVl9w5h4ZW8WSqb6MqBLeIme1mhrm4sldWibjoev/dV2a3+PIqvC/+rD6Vtvil1VDuCF0ihLZ3U3RKCw0B++yrfjZcHlHVjkMwTynKfDGv0awPNOk9jbVfZ4zg0brm/tK66163zeCgUdbmL58ky73LRAdN5Fj/hyeCxqQpMLfI8tkE3h3INv9ucymNFV+H/2kNpM8FYtC/4iCC5E6Ahos34QqXNuMME6Z4sVl5B48B4m6cz9rpumcMbpXu9bFluZc8azb8A/HJeJlx4OJwjXzHKBJDOu14BK9SFx0L+SxRM3ovsPBvlM3bHbxIg24X3j9dAWo9RErAPKRy6kygZTM9nkW0jgDe5x1nrvE0HkYc6FjbZ69zy+HO2zF9nsFgu5LdRmqab9z9H9rCykMhAlYMrlI0tsynn7GdF+K8trZOB+ayu/p7Sn3xQ59chAeJKkAaTzRI5yDE3+Lz+8vJhtAyRXyifL4dZVmAzMUPlgrLsLIJuMlCZd8nDYTp47CdDWKhkhTyFRQ6MQ1s79T2Ff0t4HfsBwXEi2R9LnjeeAG6m5zcI1ktZzEx+ddpHd6G8nLteCQ5+NsG1LCQukcc02S8D+m2Sw6Hfhqck8fXe9/Y7BTm+OinUDqRIQ6kGHCs6z95IofCC0mXBE3mscqHOyxbKg8+uheU5jJvLInm0hnvZ+rD3Pfp3hdZu80zJ2hCoUPKmPAqHxQTqGHnat7KXLeFjpCI7Q8B+QsRv08X3vfZP9EjXxcYErBXB60nwUgloodJ92TGl74bO73vn/eP94/3j/eP94/3j/eP94/3j/eP94688tG1yvLXW+d5au3G/aQb2Bb5au/G+bFljM8ZF7DyyvdAq3fjf8d4fha8LM+2Snyu0y6n8Ntt9GFTYzKjt8B7GjpOCTZwmh5m5TOtj7jI9jiyRm+v0JLNuU+NNnCaGa+3G9lB1zbMR2g2r+dZ90zzDW2vzhr6xH0821ltokdn+19tZuE9tq20/4o19Sv1J/TrOW/gwuUXZNl0HbLfSNkvz1tr+cl1Dx/HeQvsR7nwlh2GXTght58LAcyWqBG9G5cBNZBv5smXABv6jauxuN5JbCcz916Jq0HpUD1iLOkGrv6niPqtYaJ3xl38Pa3Dx0141/LeibswhWLrN3PJntxu68pmPym0NRKdlqNprK6r13o7qoTtQI3QnaoTthHGPTVC4rIbkXgp9j1JY+K9BNTrm2kFrUavHsmOmDgU5Qsu0an/0Pk7Zl/YKrWfDyGsVKvfaDCvqF2aVgzahErWKbsug6b4atQNL1ry+XcL8R7Giw3LoOSyBZc/1qBy8qXzbsu0t2HXP/ptg5DBrMdtm9lHUrxmyG6L1Qr4+M/Ye+uxHxLsVw8pvJfjO/SfcWdGp/7FjpkHs529XQPBYC6H7Ggguq6Dy3gDr7Cs32w49fbl6xN6ngvNqCA7FkHw2wSx4J2r32Y96QaugbJft8FeANYg//EhoPQsVvDehfuR2iB2H/yn4Q0qeu7UbfPps1Yj93/BjdlsFwZU+kHMxBPtl6DD0/BWvghvbP+p77Gy18F1PBcdSCNbLYNBzO6qE7kWd8J2oH1z6XNtlVNbvvU/w5PvL2vY9ctqAvlSCc8mrvmFz/93XoE3qpQdOI64eGLYOtq9v13/Z45C2/U+crRN38GvBuoj6rAh6XuvkbXXbK91XoXnfY4emH0dztk3eFtR1Gn7xROPEo2cFN9bPyyF0WgrjntvQJO7wlfiFjzJ+cXCpK36IlNxKodd9FdTsGi63lbDosQG98s/w38Zafh4mA5Y+iTP33/xCz3UFFB5yJ9WOO46aPYs/FxoOEN8GVtz8b1MFm6X8wgmBvLdmn4Oo5ld49m32MesgalQP2/WD4LScH7MeHZO+13q0TdiSXLbO1L0wS1/5pE+9qAPfCDaLqcNWooLHGliG7Uez+P0wtS9Y/kfvk7jw24wK9MVgs5t537iWwNJ/IxJn32r0e9stPAFt+PR70ys40pfJYSm/ZJZtX6F7KY8IXZM2/suNm8NmfLmAebXSeRX8x18vmrgTv30PKiLc0jJ4O7/qhN1Lgk3jqhu1F0Kz9F/8mJnLyCsb2B0C5PtNbIB+z62oGbIFBvbjtW/R0Zq60Qd+ZleTlN27Qt9/Mz6K2wuN9eiebwOt1cATXwrOy/k+NH7rUCvqAITO4//lJx3nHkHNhrEHHrHLjEwCNvK5GcYUJZr3PQRt19GJv/cemat/dDNgV7B4r5Hfh0JunQiKSE1Tq/6ZY0wpepJq4LWBvJS9N31edv+OwK2oHbEbQsO0c2Xr+Yy7sZalpxpB2zFszfOI391pzvqfHapSDjCkmFux91YYB21B076HYew4ufnr64XMeLiMfcPYOlYh2yBQfuuace6tPCNq9teTBadi+uDrUIn2wcyY8mXDvifxYe/SL//sfqbug7ZNyqmv2d142PGYBW1G3bjDqBW4/DdvB5Re+lOUcY+NMKU8zd6Tdf6HicdRP3zDM6H5UPWb3ie15Psgi6CtMKGcwt+n13Z8lLAPGttxDf7ssWav+yG6IqUdFpmsQrfDnHKUKe2nXvwBtE/cdrp58uHTgn0J9eWFh7OPod0f7pCAOdeN2Uc720bf0l2oErET7VJOELAJjkK9ZEOhcmQN+9SDPi5jbn1r0WsLavTZye8BXyVkB3LW/9jrzx74pD2wqt1nN+rG7n3pmH3lRzM68NrRu8l2oWrkLnrPk9DaFwz+M/uavBcGndPOfsM6kh1ztcgdaNr/BOoEr/hNYFOoj9sMPvm9GSV9tj57z+pRe9Bm4FEIrd+ch1NXPg+pEUHHF76db1eDtmmefAjGTpMavM0XNW/zz70/jN7P75jzAX3mGnS8NaP3oEHSUZiR53VNPX7wT+8sd8sL907pZ9A85SjaDz2JVqknYJdzHm367fzp4+gNP3QZchDtBh/Hx/ThOmaeRmtazzXvs/OzjiHxbQ46vvDJKhYShm3A1NLP0KFlynE0HXAI7dNOovXgo3DOv4lOA3b/LDQb8ofXVE0+ACOP/KvfNux3kB9z69TjcBp5BU2j1r3xhlu+BTc+a5B8gK/PjH1eu+wL8B19uvebtsla/0OftkNOouXgY3ybdhlnYD/sLAwdJzZ4W6E17zh87XIuPmna/xDa0WduN/QEmg04DLcxN+CedfiCcZfhH/6pHWWueuFUh2hX6b0F1SNJHuu+TU6jbqHVUFIc5L6VKBxUpdhdmzyhPXmCbb9Nc9/mYEdRWqxOMrxR4uHym2L5jb2xoFIQe88d3Gurhe+gL8MpUB31h9OdC7bCoGPqmW+sKIyzY64WRqEq+RjJ96I3ArPNunC+Eklqtj6zKhSePqD39c476fimbQYt/b53dYo4VSn68L6hz/9xX/q2OE56a2C8LCl+2od5LLt1Ld9fHxbRdrB+QVVfUod1+9r84U76zfrZml+52GkBl8ZC50W8BnMefDQgtwQd2g04s1toNQOCTSHZElRwKYWlHym8Rin5f/ZAO6Vc2SM0m4YmMfvvVfOcVyC2y5vbJm7HWkOfza/e15qUnPsG1Om9AXptM3+3VhpVBE2N0D1fCq1n6rYtJCVHYshlzhuBVQ3Z/aB8fSadKclbBGx72il+q+ZN20RP/s6Pr991ke59lsCcFLTxXwD2YdBqFwvvdReZQtZnV5O2mwPBbqm8366FsAzejfqh66Fsm9ntd3c0aP4LW5XXGi4/maRnN/KyokQrtMkqv1V324HnPhXaEjSqIQRH9nNRm9CQYrF+59yBf1g7Lfipi+BIHdR1IfR91kPjQ8rQbwvUTNa7UI1jt4Qkd6m8bzr4ylQvWXnO/d3fKxm3Gvp1Ivd9JXRZoDvmYl63mHefn/Fb648oRn2tL6k1+yW62o3et+1MuOXc/t1rkGOnPe3B90/lA2+7laBK8BYG7K1uY95xwJGhEm0rdF6MmFnPRk3fjHb1og49FNrPpj5YIX92+hJZBO1C/TBSsu2yfN+4s7Fr0USfpCv/1rmX8kKxethuaGzGltcai0gw1Ajd+5K/gSfVI45F1OnbOTRNpxG/O6e87eArl4Q2M+GV96AgfuY3PWLmvIyNmPIk3mfUg/iURT/G14099kjospCgEUDXVaSmNuJDChNixxzr39tvk6RTX/Dt2DFTZ5gE7oCp29zf/K3kXpOfrhS6Mg9eLQNrPR3Vw/f9MGYVLH9XkJXAnu+flC1vKbpUodBuYFfwp+4katltqunHCQeL9N0pFAfu/Krfgp/Kfwdz0gZU6px65Zhgu0Qu/tn+HYthFrgLDSK3Q9M5N/5f91gr2tAr51KEeQB5lN0i+YJuatmQT+2ey6YLTQZVF6zCeZ1VsBEuIvOUjszTVvD1jMhbGtHOLZynjhTaZH8g1ImXynataDakaou4g9MqOBXBnIrNLn13ef5muBx4fLR+N+oQRzpwF/n9zag2qx+2/jt1p1Hdab//0jkdE3d91CTx+DOhyxz5mGlbI/KgeTj20gAAAvlJREFUD4JLz6g7jbRWdhldU2Mzrm4l52nuNQNKl6iZB9sukI+9eQHqhG5/nFOENr/b2436m3rmXMnQ92Zf0EJd3yxGDeqbqt4LRgotM6zo2H5TIBlaj23yQY/lA+qGffKlgvrMwK0EzplnJ/M/1ogqnxpnm7ynduPYg/cFm4Xy/nWf39h3IxpEbENlt9mFUpcxnel9zAXXtFP5Bq2zKDSMhNByOFmObM2HUTgcCSOn6bDymEVhb0z5FYUpi55kCi2yITROhdCKtvkojTphHAmF2bB0oc6om8R/Hjhm8t1UrfV4CC1ofw0GUyjIh9iFrNMYCFoP/uNn83ZDW8lz/k46mB+FjvkQPs6S378VWdMMSDaTYeW9EOqu+eVjjaNWoo2Fy/SbQqth8vuzYyk/7myo7KfAyHkaDO3GQd+2AFpnBoj+9kEy329F17nwyDizKG8VfjdHuqafytO0zCBPHPFq/8zYMbYfDRPn6ajqMRtq24JZvxhLnP7I3txh0kOt3SToO1K+/JiO76OhENgvw3fOg9R5NPVRIh/Mdcs4PUVomvJY6zj11Wfnn58+V5N0ynFjYOIyB2aOk1HRbfb3gv3Agzer2o2HqdPknyq6Tnts5T7jsZUbmfvMxxbdpjw2c5r8XW2vefAdcWXa6wfllfPpDCub0Y+t2DbdZz42d5782LRz3pNW0Vu+Csi/xn/twCf700OWHXNh5jz5O6vusx5bOE95XMlm7HO7gYevBOSel3Sdb9ksfC0qWY95aUnvx95Xfv8ZjyuRmTpMeFzVZfpz94yzeWXv3XfmszASLbBwmAiLX2/jNv2xqdOkxxXdZ/1Q1Xs+qnrNf17VY879hgHLztr03bkxNO+zlGHLUP/PhLJuQ45dqGKdz/rmh0qu01/1DX1eC/q85g4Tn9YLLEJQ3s1flDbh4+8PqdJtGqjvXpg4TnrCPgffho7V0m78900j1qHn6Jv12LrU/7eq2oxl6z4r37/bawxcpj6mfTy2shv/o8Ogw2f+H+nj8uzAfcGVAAAAAElFTkSuQmCC\" title=\"logo\" alt=\"logo\"> </a> </td> </tr> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td> <table width=\"95%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:670px;background:#fff; border-radius:3px; text-align:center;-webkit-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);-moz-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);box-shadow:0 6px 18px 0 rgba(0,0,0,.06);\"> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> <tr> <td style=\"padding:0 35px;\"> <h1 style=\"color:#1e1e2d; font-weight:500; margin:0;font-size:32px;font-family:'Rubik',sans-serif;\">You have requested to Join Rx-Splitter Group.</h1> <span style=\"display:inline-block; vertical-align:middle; margin:29px 0 26px; border-bottom:1px solid #cecece; width:100px;\"></span> <p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\">Hey Hello,</br> " + user.Name + " (" + user.Email + ")" + " has invited you to join group on Rx-Splitter. A unique link to reset your password has been generated for you. To reset your password, click the following link and follow the instructions. </p>";
                var userData = _unitOfWork.User.GetT(x => x.Email == Email);

                if (userData != null)
                {
                    body += "<button><a href=\"https://rxsplitterapis.azurewebsites.net/api/v1/MemberInvitation/MemberInvitationAccept/" + tokenString + "\"+ tokenString + \"'\" style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Accept</a></button>";
                }
                else
                {
                    body += "<button><a href=\"https://rxsplitapp.azurewebsites.net/#/Login/" + tokenString + "\"+ tokenString + \"'\" style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Join Rx-Splitter</a></button>";
                }

                body += "<button><a href=\"https://rxsplitterapis.azurewebsites.net/api/v1/MemberInvitation/MemberInvitationDecline/" + tokenString + "\"+ tokenString + \"'\" style=\"background:red;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Decline</a></button>";

                body += "</td> </tr><tr><td><p style=\\\"color:#455056; font-size:15px;line-height:24px; margin:0;\">Rx-Splitter makes it easy to split expenses with your friends. </p></td></tr><tr><td><p style=\\\"color:#455056; font-size:15px;line-height:24px; margin:0;\">SignUp Now to see all your group's biils and to start adding bills to your own. </p></td></tr> <tr> <td style=\"height:40px;\">&nbsp;</td> </tr> </table> </td> <tr> <td style=\"height:20px;\">&nbsp;</td> </tr> <tr> <td style=\"text-align:center;\"> <p style=\"font-size:14px; color:rgba(69, 80, 86, 0.7411764705882353); line-height:18px; margin:0 0 0;\">&copy; <strong>https://rxsplitapp.azurewebsites.net</strong></p> </td> </tr> <tr> <td style=\"height:80px;\">&nbsp;</td> </tr> </table> </td> </tr> </table> <!--/100% body table--> </body> </html>";
                //string body = "<a href='https://rxsplitapp.azurewebsites.net/#/Login/'"+ tokenString + "'><input type='button' value='Reset Password' /></a>";
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

        [HttpPost("DeleteGroupMember")]
        public IActionResult DeleteGroupMember(int memberId, int groupId)
        {
            var groupMember = _unitOfWork.GroupMember.DeleteMemberByGroupId(memberId, groupId);
            if (groupMember != null)
            {
                _unitOfWork.Save();
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The Group Member Data deleted Successfully." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The Group Member Data given by you is totally empty.." });
            }
        }

        [HttpPost("AddInitialSummary")]
        public string AddInitialSummary(JsonElement expense)
        {
            Summary summary = new Summary();
            var result = _expenseService.AddInitialSummary(summary);
            return "";
        }


    }
}
