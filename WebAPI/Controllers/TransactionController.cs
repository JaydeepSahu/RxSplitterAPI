using AutoMapper;
using DomainLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service_Layer.UnitOfWork;
using System.Security.Claims;
using System.Text.Json;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class TransactionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TransactionController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost("AddTransaction/{GroupId}")]
        public async Task<IActionResult> AddTransaction(int GroupId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;  
            if (GroupId != null && userClaims != null && userClaims.Count()>0)
            {
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Transaction.AddTransaction(UserId, GroupId);
                if (obj == false)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }
        [HttpPost("AddTransactionbyCsharp/{GroupId}")]
        public async Task<IActionResult> AddTransactionbyCsharp(int GroupId, JsonElement expense)
        {
            //var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
           
                //Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Transaction.AddTransactionbyCsharp(GroupId,expense);
                if (obj == false)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }

        [HttpPost("GetSettleUpPaymentData/{GroupId}")]
        public async Task<IActionResult> GetSettleUpPaymentData(int GroupId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
            if (GroupId != null && userClaims != null && userClaims.Count() > 0)
            {
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Transaction.GetSettleUpPaymentData(UserId, GroupId);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }

        [HttpPut("AcceptTransaction/{TransactionId}")]
        public async Task<IActionResult> AcceptTransaction(int TransactionId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
            if (TransactionId != null && userClaims != null && userClaims.Count() > 0)
            {
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                bool obj = await _unitOfWork.Transaction.AcceptTransaction(UserId, TransactionId);
                if (obj == false)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }

        [HttpGet("GetTransactionAccGroupId/{GroupId}")]
        public async Task<IActionResult> GetTransactionAccGroupId(int GroupId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
            if (GroupId != null && userClaims != null && userClaims.Count() > 0)
            {
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Transaction.GetTransactionDataAccGroupId(UserId, GroupId);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }


        [HttpGet("GetExpenseSummaryDetailAccGroupId/{GroupId}")]
        public async Task<IActionResult> GetExpenseSummaryDetailAccGroupId(int GroupId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
            if (GroupId != null && userClaims != null && userClaims.Count() > 0)
            {
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Transaction.GetExpenseSummaryDetailAccGroupId(UserId, GroupId);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }
    }
}
