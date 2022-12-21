using DomainLayer.Data;
using DomainLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Service_Layer.UnitOfWork;
using System.Security.Claims;
using System.Security.Principal;
using WebAPI.Exceptions;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DomainLayer.DTO;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GroupController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpGet(nameof(GetAllGroups))]
        public IActionResult GetAllGroups()
        {
            //var obj = _mapper.Map<GroupDTO>(_unitOfWork.Group.GetAll());
            var obj = _unitOfWork.Group.GetAll();
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
        }
        [HttpGet(nameof(GetAllGroupsOfUser))]
        public IActionResult GetAllGroupsOfUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = _unitOfWork.Group.GetByExpression(x => x.AddedBy == UserId);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
        }

        [HttpGet("GetGroupDetailsById/{GroupId}")]
        public IActionResult GetGroupDetailsById(int GroupId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                List<Group> obj = _unitOfWork.Group.GetGroupDataWithMembersByGroupId(GroupId);
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
        }
        [HttpPost(nameof(CreateGroup))]
        public IActionResult CreateGroup(Group group)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (group != null && identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                using (var context = new RxSplitterContext())
                {
                    group.AddedBy = UserId;
                    bool res = _unitOfWork.Group.Insert(group);
                    if (res)
                    {
                        _unitOfWork.Save();
                        int groupId = group.Id;
                        var groupData = _unitOfWork.Group.Get(groupId);
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = groupData });
                    }
                    else
                    {
                        return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "Operation Failed." });
                    }
                }
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "The User Data given by you is totally empty.." });
            }
        }
        [HttpPut(nameof(UpdateGroup))]
        public IActionResult UpdateGroup(GroupDTO group)
        {
            if (group != null)
            {
                bool res = _unitOfWork.Group.Update(_mapper.Map<Group>(group));
                if (res)
                {
                    _unitOfWork.Save();
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = "The Group Data updated Successfully." });
                }
                return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "Operation Failed." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "The Group Data given by you is totally empty.." });
            }
        }

        [HttpDelete("DeleteGroup/{GroupId}")]
        public IActionResult DeleteGroup(int GroupId)
        {
            var group = _unitOfWork.Group.Get(GroupId);
            if (group != null)
            {
                bool res = _unitOfWork.Group.Delete(group);
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
