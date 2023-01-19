using DomainLayer.Data;
using DomainLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Service_Layer.ICustomServices;
using Service_Layer.UnitOfWork;
using WebAPI.Exceptions;
using System.Text.RegularExpressions;
using AutoMapper;
using DomainLayer.DTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors("CorsPolicy")]
    [ApiVersion("1.0")]
    [ApiController]
    public class UserDetailController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserDetailController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("GetUserById/{Id}")]
        public async Task<IActionResult> GetUserById(Guid Id)
        {

            var obj = await _unitOfWork.User.GetByExpression(x => x.Id == Id);
            if (obj == null)
            {
                return NotFound($"The User Data with this ID {Id} not found in the system.");
            }
            else
            {
                return Ok(obj);
            }
        }

        [HttpGet("CheckExistedUser/{Email}")]
        public async Task<IActionResult> CheckExistedUser(string Email)
        {
            var obj = await _unitOfWork.User.GetByExpression(x => x.Email == Email);
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "User Doesn't Exist." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "User Exists." });
            }
        }

        //[Authorize]
        [HttpGet(nameof(GetAllUsers))]
        public async Task<IActionResult> GetAllUsers()
        {

            var query = await _unitOfWork.User.GetAll();
            var obj = _mapper.Map<List<UserDetailDTO>>(query);


            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "The User Data Not Found." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
        }

        [HttpPost(nameof(CreateUser))]
        public async Task<IActionResult> CreateUser(UserDetailDTO user)
        {
            if (user != null)
            {
                user.Password = CommonMethods.Encryptword(user.Password);
                var obj = _mapper.Map<UserDetail>(user);
                var existedUser = await _unitOfWork.User.GetByExpression(x => x.Email == user.Email);
                if (existedUser != null && existedUser.Count > 0)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "User Email Already Exists." });
                }
                else
                {
                    obj.AddedOn = DateTime.UtcNow;
                    _unitOfWork.User.Insert(obj);
                    _unitOfWork.Save();

                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity != null && identity.Claims != null && identity.Claims.Count() > 0)
                    {
                        var userClaims = identity.Claims;
                        int GroupId = Convert.ToInt32(userClaims.FirstOrDefault(x => x.Type == "GroupId").Value);
                        string Email = userClaims.FirstOrDefault(x => x.Type == "Email").Value;

                        var mbr = _unitOfWork.GroupMember.GetT(x => x.GroupId == GroupId && x.Email == Email);
                        mbr.UserId = obj.Id;
                        mbr.UpdatedOn = DateTime.UtcNow;
                        _unitOfWork.GroupMember.Update(mbr);
                        await _unitOfWork.MemberInvitation.AccectInvitation(mbr);
                    }
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "Created Successfully." });
                }

            }

            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "The User Data given by you is totally empty.." });
            }
        }

        [Authorize]
        [HttpPut(nameof(UpdateUser))]
        public IActionResult UpdateUser(UserDetailDTO user)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                user.Id = UserId;
                if (user != null)
                {

                    bool res = _unitOfWork.User.Update(_mapper.Map<UserDetail>(user));
                    //bool res = _unitOfWork.User.Update(user);

                    if (res)
                    {
                        _unitOfWork.Save();
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The User Data updated Successfully." });
                    }
                }
                return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "Operation Failed." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The User Data given by you is totally empty.." });
            }
        }

        [Authorize]
        [HttpDelete(nameof(DeleteUser))]
        public IActionResult DeleteUser(Guid UserId)
        {
            var user = _unitOfWork.User.GetT(x => x.Id == UserId);
            if (user != null)
            {
                bool res = _unitOfWork.User.Delete(user);
                if (res)
                {
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The Group Data deleted Successfully." });
                }
                return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "The Group Data Updation Failed." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The Group Data given by you is totally empty.." });
            }
        }

        [Authorize]
        [HttpGet(nameof(GetUserDetailById))]
        public async Task<IActionResult> GetUserDetailById(Guid Id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var userClaims = identity.Claims;
            Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
            Id = UserId;
            var obj = await _unitOfWork.User.GetByExpression(x => x.Id == Id);
            if (obj == null)
            {
                return NotFound($"The User Data with this ID {Id} not found in the system.");
            }

            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
        }
    }
}

