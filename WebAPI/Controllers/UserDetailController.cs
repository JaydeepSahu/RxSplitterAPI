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

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class UserDetailController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserDetailController(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("GetUserById/{Id}")]
        public IActionResult GetUserById(Guid Id)
        {
            
            var obj = _unitOfWork.User.GetByExpression(x=>x.Id==Id);
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
        public IActionResult CheckExistedUser(string Email)
        {
            var obj = _unitOfWork.User.GetByExpression(x => x.Email == Email);
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "User Doesn't Exist." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "User Exists." });
            }
        }

        [Authorize]
        [HttpGet(nameof(GetAllUsers))]
        public IActionResult GetAllUsers()
        {
            //var obj = ;

            //var UserDetailDTO = new UserDetailDTO();
            //var obj =  _mapper.Map<UserDetail>(UserDetailDTO);
           
            var obj=_unitOfWork.User.GetAll();
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
        public IActionResult CreateUser(UserDetailDTO user)
        {
            if (user != null)
            {
                user.Password=CommonMethods.Encryptword(user.Password);

                var obj = _mapper.Map<UserDetail>(user);
                var existedUser=_unitOfWork.User.GetByExpression(x=>x.Email==user.Email);
                if (existedUser == null)
                {
                    _unitOfWork.User.Insert(obj);
                    _unitOfWork.Save();
                    return Ok("Created Successfully");
                }
                else
                {
                    return Ok("User Email Already Exists.");
                }
            }
            else
            {
                throw new BadRequestException($"The User Data given by you is totally empty..");
            }
        }

        [Authorize]
        [HttpPut(nameof(UpdateUser))]
        public IActionResult UpdateUser(UserDetail user)
        {
            if (user != null)
            {
                bool res = _unitOfWork.User.Update(user);
                if (res)
                {
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The User Data updated Successfully." });
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
            var user = _unitOfWork.User.GetT(x=>x.Id==UserId);
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
    }
}
