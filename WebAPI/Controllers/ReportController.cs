using AutoMapper;
using DomainLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service_Layer.UnitOfWork;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class ReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReportController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("getExpensesChart")]
        public async Task<IActionResult> getExpensesChart()
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;
          
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = await _unitOfWork.Report.GetAllExpensesChart(UserId);
                if (obj == null)
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
                }
                else
                {
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
                }
         
            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }
        [HttpGet("GetGroupExpensesChart")]
        public async Task<IActionResult> GetGroupExpensesChart(int groupId)
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;

            Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
            var obj = await _unitOfWork.Report.GetGroupExpensesChart(UserId, groupId);
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }

            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }

        [HttpGet("GetMonthwiseExpenses")]
        public async Task<IActionResult> GetMonthwiseExpenses(int year )
        {
            var userClaims = (HttpContext.User.Identity as ClaimsIdentity).Claims;

            Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
            var obj = await _unitOfWork.Report.GetMonthwiseExpenses(UserId, year);
            if (obj == null)
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }

            return Ok(new APIResponse { StatusCode = StatusCodes.Status404NotFound.ToString(), Status = "Failure", Response = "No Data Found" });
        }

    }
}
