using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.AspNetCore.Http;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Repository_Layer.IRepository;
using System.Dynamic;
using System.Text.Json;
using Newtonsoft.Json;

namespace Service_Layer.CustomServices
{
    public class ExpenseService : Repository<Expense>, IExpenseService
    {
        private readonly RxSplitterContext _context;
        private readonly IMapper _mapper;
        private readonly ISprocRepository _sprocRepo;
        public ExpenseService(RxSplitterContext context, IMapper mapper, ISprocRepository sprocRepo) : base(context)
        //public ExpenseService(RxSplitterContext context) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _sprocRepo = sprocRepo;
        }

        public async Task<ExpensesDTO> SaveExpenseData(ExpensesDTO expense)
        {
            try
            {
                Expense obj = new Expense();
                obj = _mapper.Map<Expense>(expense);
                obj.ExpenseNumber = "EXP" + DateTime.UtcNow.Ticks;
                _context.Add(obj);
                _context.SaveChanges();
                if (expense.lstExpenseTransaction != null && expense.lstExpenseTransaction.Count > 0)
                {
                    foreach(ExpenseTransactionDTO item in expense.lstExpenseTransaction)
                    {
                        item.ExpensesId = obj.Id;
                        item.PaidByMemberId = expense.PaidBy;
                        item.TransactionNumber = "TXN" + DateTime.UtcNow.Ticks;
                        var expenseTransaction = _mapper.Map<ExpenseTransaction>(item);
                        _context.ExpenseTransactions.Add(expenseTransaction);
                        _context.SaveChanges();
                    }
                    //var objTransaction = expense.lstExpenseTransaction;
                }
                return _mapper.Map<ExpensesDTO>(_context.Expenses.First(x => x.Id == obj.Id));

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<IEnumerable<dynamic>> GetAllExpensesAccGroupId(int GroupId, Guid UserId, string Mode)
        {
            
            dynamic model = new ExpandoObject();
            model.lstExpenseTransaction = new List<SP_GetExpenseTransactionDetailAccExpenseId>();
            var obj = _sprocRepo.GetStoredProcedure("SP_GetAllExpensesDetailsAccGroupId")
                .WithSqlParams((nameof(GroupId), GroupId),
                (nameof(UserId), UserId),
                (nameof(Mode), Mode))
                .ExecuteStoredProcedureAsync<SP_GetAllExpensesDetailsAccGroupId>();
            List<ExpensesDTO> a =_mapper.Map<List<ExpensesDTO>>(obj.Result);
            
            for (int i = 0; i < a.Count; i++)
            {
                a[i].lstTransaction = new List<dynamic>();
                var x = _sprocRepo.GetStoredProcedure("SP_GetExpenseTransactionDetailAccExpenseId")
                .WithSqlParams(("ExpenseId", a[i].Id))
                .ExecuteStoredProcedureAsync<SP_GetExpenseTransactionDetailAccExpenseId>();
                a[i].lstTransaction.AddRange(x.Result);
            }
            return a;
        }

        public async Task<IEnumerable<dynamic>> GetAllExpensesAccUserId( Guid UserId, string Mode)
        {

            dynamic model = new ExpandoObject();
            model.lstExpenseTransaction = new List<SP_GetExpenseTransactionDetailAccExpenseId>();
            var obj = _sprocRepo.GetStoredProcedure("SP_GetAllExpensesDetailsAccUserId")
                .WithSqlParams(
                (nameof(UserId), UserId),
                (nameof(Mode), Mode))
                .ExecuteStoredProcedureAsync<SP_GetAllExpensesDetailsAccGroupId>();
            List<ExpensesDTO> a = _mapper.Map<List<ExpensesDTO>>(obj.Result);

            for (int i = 0; i < a.Count; i++)
            {
                a[i].lstTransaction = new List<dynamic>();
                var x = _sprocRepo.GetStoredProcedure("SP_GetExpenseTransactionDetailAccExpenseId")
                .WithSqlParams(("ExpenseId", a[i].Id))
                .ExecuteStoredProcedureAsync<SP_GetExpenseTransactionDetailAccExpenseId>();
                a[i].lstTransaction.AddRange(x.Result);
            }
            return a;
        }

        public async Task<int> AddSummary(JsonElement expense)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(expense);
            ExpensesDTO expenseDTO = JsonConvert.DeserializeObject<ExpensesDTO>(json);
            //var obj = _unitOfWork.Expense.GetAllExpensesAccGroupId(GroupId);
            int paidBy = expenseDTO.PaidBy;
            decimal totalAMount = expenseDTO.Amount;
           

            var list = new List<Summary>();
            foreach (ExpenseTransactionDTO item in expenseDTO.lstExpenseTransaction)
            {
                decimal sharedAmount = (decimal)item.Amount;
                Summary summary = new Summary();
                if (item.ParticipantMemberId == paidBy)
                {
                    var details = _context.Summaries.FirstOrDefault(x => x.ParticipantId == item.ParticipantMemberId && x.IsActive==true);
                       if(details!=null)
                        {
                        details.IsActive = false;
                        summary.RemainingAmount = details.RemainingAmount+(totalAMount-sharedAmount);
                        summary.IsActive= true;
                        _context.Summaries.Update(details);
                    }
                    else
                    {
                       // details.RemainingAmount = 0;
                        summary.RemainingAmount = 0 + (totalAMount - sharedAmount);
                        summary.IsActive = true;

                    }
                }
                else
                {
                    var details = _context.Summaries.FirstOrDefault(x => x.ParticipantId == item.ParticipantMemberId && x.IsActive == true && x.IsDelete==false);
                    if (details!= null)
                    {
                        details.IsActive = false;
                        summary.RemainingAmount = details.RemainingAmount  - sharedAmount;
                        summary.IsActive = true;
                        _context.Summaries.Update(details);
                    }
                    else
                    {
                        //details.RemainingAmount = 0;
                        summary.RemainingAmount = 0  - sharedAmount;
                        summary.IsActive = true;

                    }
                    
                }
                summary.ParticipantId = item.ParticipantMemberId;
                summary.GroupId = expenseDTO.GroupId;
                list.Add(summary);

               
            }

             _context.Summaries.AddRange(list);

            return  _context.SaveChanges();
        }

        public async Task<IEnumerable<Summary>> GetSummaryByGroupID(int GroupId)
        {
            var result = _context.Summaries.Where(x => x.GroupId == GroupId && x.IsActive==true && x.IsDelete ==false).ToList();
            return result;
        }

        public async Task<int> AddInitialSummary(Summary summary)
        {
            _context.Summaries.Add(summary);
            var result = _context.SaveChanges();
            return result;
        }

        public  int UpdateSummary(int memberId, int GroupId)
        {
            var updateisdel = _context.Summaries.FirstOrDefault(x => x.ParticipantId == memberId && x.GroupId == GroupId && x.IsDelete == true);
            if(updateisdel != null)
            {
                updateisdel.IsDelete = false;
                updateisdel.IsActive = true;
                _context.Summaries.Update(updateisdel);
            }
            
            return _context.SaveChanges();
        }

        public Task<IList<SP_totalGroupOweAmount>> GetUserDashboardDetails(Guid UserId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_totalGroupOweAmount")
                .WithSqlParams(
                (nameof(UserId), UserId))
                .ExecuteStoredProcedureAsync<SP_totalGroupOweAmount>();
            //List<ExpensesDTO> a = _mapper.Map<List<ExpensesDTO>>(obj.Result);
            return obj;
        }
    }
}
