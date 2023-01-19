using DomainLayer.Data;
using DomainLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Service_Layer.UnitOfWork;
using System.Security.Claims;
using System.Security.Principal;
using WebAPI.Exceptions;
using Service_Layer.CustomServices;
using Service_Layer.ICustomServices;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DomainLayer.DTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using AutoMapper.Execution;
using System.Text.RegularExpressions;
using Group = DomainLayer.Data.Group;
using Microsoft.AspNetCore.Cors;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IExpenseService _expenseService;
        public GroupController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet(nameof(GetAllGroups))]
        public async Task<IActionResult> GetAllGroups()
        {
            var obj = await _unitOfWork.Group.GetAll();
            //var obj = await _unitOfWork.Group.GetAll().Result.Select(x => new GroupDTO
            //{
            //    GroupName = x.GroupName,
            //    GroupImagePath = x.GroupImage
            //});
            //var obj = _mapper.Map<List<GroupDTO>>(query);
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
                
                //var obj = _unitOfWork.Group.GetByExpression(x => x.AddedBy == UserId);

                var obj =  _unitOfWork.Group.GetAllDetailGroupsOfUser(UserId);

                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj.Result});
                }
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
        }

        [HttpGet(nameof(GetAllGroupsForAll))]
        public IActionResult GetAllGroupsForAll()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);

                //var obj = _unitOfWork.Group.GetByExpression(x => x.AddedBy == UserId);

                var obj = _unitOfWork.Group.GetAllDetailGroups(UserId);

                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj.Result });
                }
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
        }

        [HttpGet("GetGroupDetailsById/{GroupId}")]
        public async Task<IActionResult> GetGroupDetailsById(int GroupId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                List<Group> obj = await  _unitOfWork.Group.GetGroupDataWithMembersByGroupId(GroupId);
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
        }

        [HttpPost(nameof(CreateGroup))]
        public async Task<IActionResult> CreateGroup(Group group)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
            if (group != null && userClaims != null)
            {
                //var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                using (var context = new RxSplitterContext())
                {
                    group.AddedBy = UserId;
                    var mappedMember = _mapper.Map<Group>(group);

                  // group.CurrencyId=CurrencyId;
                    bool res =  _unitOfWork.Group.Insert(group);
         

                    if (res)
                    {
                        _unitOfWork.Save();
           
                        GroupMember gm = new GroupMember() { Email = userClaims.FirstOrDefault(x => x.Type == "Email").Value, GroupId = group.Id,UserId=UserId };
                        _unitOfWork.GroupMember.Insert(gm);
                        _unitOfWork.Save();

                        MemberInvitation mi = new MemberInvitation() { MemberId = gm.Id, GroupId = group.Id, TokenGeneratedByUser = UserId, InvitationStatus="1" };
                        _unitOfWork.MemberInvitation.Insert(mi);
                        _unitOfWork.Save();

                        int groupId = group.Id;
                        var groupData = _unitOfWork.Group.Get(groupId);
                        Summary summary = new Summary();
                        summary.ParticipantId = gm.Id;
                        summary.RemainingAmount = 0;
                        summary.GroupId = group.Id;
                        summary.IsActive = true;
                        summary.IsDelete = false;
                        var result = _unitOfWork.Expense.AddInitialSummary(summary);
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

        [HttpGet(nameof(GetAllCategories))]
        public async Task<IActionResult> GetAllCategories()
        {
            var obj = await _unitOfWork.Group.GetAllCategories();
       
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
        }



        [HttpGet(nameof(GetAllCurrencies))]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var obj = await _unitOfWork.Group.GetAllCurrency();

            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
        }
    }
}
