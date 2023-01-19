using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service_Layer.CustomServices;
using Service_Layer.ICustomServices;
using Service_Layer.UnitOfWork;
using System.Security.Claims;
using System.Text.Json;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ExpenseController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost("AddExpense")]
        public IActionResult AddExpense([FromBody] JsonElement expense)
        {
            
            string json = System.Text.Json.JsonSerializer.Serialize(expense);
            ExpensesDTO expenseDTO = JsonConvert.DeserializeObject<ExpensesDTO>(json);
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (expenseDTO != null && identity != null)
            {
                var userClaims = identity.Claims;
                expenseDTO.AddedBy = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var res = _unitOfWork.Expense.SaveExpenseData(expenseDTO);
                if (res != null)
                {
                    string c = AddSummary(expense);
                    return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = res.Result });
                }
                return Ok(new APIResponse { StatusCode = StatusCodes.Status500InternalServerError.ToString(), Status = "Failure", Response = "Insertion Failed." });
            }
            else
            {
                return Ok(new APIResponse { StatusCode = StatusCodes.Status400BadRequest.ToString(), Status = "Failure", Response = "Data Sent by You is Null." });
            }
        }

        [HttpGet("GetAllExpensesAccGroupId/{Mode}/{GroupId}")]
        public IActionResult GetAllExpensesAccGroupId(string Mode,int GroupId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = _unitOfWork.Expense.GetAllExpensesAccGroupId(GroupId,UserId, Mode);
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj });
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "Unauthorized Access" });
        }

        [HttpGet("GetAllExpensesAccUserId/{Mode}/{PageNumber}/{Entity:int=10}")]
        public IActionResult GetAllExpensesAccUserId(string Mode , int PageNumber = 1, int Entity=10)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = _unitOfWork.Expense.GetAllExpensesAccUserId(UserId, Mode);
                var a = obj.Result.ToList();
                int TotalPages = Convert.ToInt32(Math.Ceiling(a.Count / Convert.ToDecimal(Entity)));
                var pagenumber = PageNumber;
                a = a.Skip((PageNumber - 1) * Entity).Take(Entity).ToList();
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = new APIResponse { StatusCode = TotalPages.ToString(), Status = obj.Result.ToList().Count.ToString(), Response = a  } });
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "Unauthorized Access" });
        }

            [HttpPost("AddSummary")]
        public string AddSummary(JsonElement expense)
        {
            Summary summary = new Summary();
            var obj = _unitOfWork.Expense.AddSummary(expense);

            return "";
        }

        [HttpGet("GetSummary")]
        public IActionResult GetSummary(int GroupId)
        {
            Summary summary = new Summary();
            var obj = _unitOfWork.Expense.GetSummaryByGroupID(GroupId);

            return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj.Result });
        }


        [HttpGet("GetUserDashboardDetails")]
        public IActionResult GetUserDashboardDetails()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                Guid UserId = new Guid(userClaims.FirstOrDefault(x => x.Type == "Id").Value);
                var obj = _unitOfWork.Expense.GetUserDashboardDetails(UserId);
                return Ok(new APIResponse { StatusCode = StatusCodes.Status200OK.ToString(), Status = "Success", Response = obj.Result });
            }
            return Ok(new APIResponse { StatusCode = StatusCodes.Status406NotAcceptable.ToString(), Status = "Failure", Response = "Unauthorized Access" });
        }
    }
}
