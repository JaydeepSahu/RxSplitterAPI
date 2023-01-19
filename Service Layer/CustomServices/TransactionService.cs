using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repository_Layer.IRepository;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class TransactionService : Repository<Transaction>, ITransactionService
    {
        private readonly RxSplitterContext _context;
        private readonly IMapper _mapper;
        private readonly ISprocRepository _sprocRepo;
        public TransactionService(RxSplitterContext context, IMapper mapper, ISprocRepository sprocRepo) : base(context)
        //public ExpenseService(RxSplitterContext context) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _sprocRepo = sprocRepo;
        }

        public async Task<bool> AcceptTransaction(Guid UserId, int TransactionId)
        {
            var trans = _context.Transactions.FirstOrDefaultAsync(x=>x.Id==TransactionId);
            if (trans!=null)
            {
                trans.Result.Status = "1";
                trans.Result.UpdatedOn= DateTime.UtcNow;
                var groupDetails = _context.Summaries.First(x => x.ParticipantId == trans.Result.PaidFromMemberId);
                var paidFrom = _context.Summaries.First(x => x.GroupId == groupDetails.GroupId && x.IsActive == true && x.ParticipantId == trans.Result.PaidFromMemberId);
                var paidTO = _context.Summaries.First(x => x.GroupId == groupDetails.GroupId && x.IsActive == true && x.ParticipantId == trans.Result.PaidToMemberId);

                if (trans.Result.Amount > 0)
                {
                    paidFrom.RemainingAmount = paidFrom.RemainingAmount + trans.Result.Amount;


                    paidTO.RemainingAmount = paidTO.RemainingAmount - trans.Result.Amount;
                }
                else
                {
                    paidFrom.RemainingAmount = paidFrom.RemainingAmount - trans.Result.Amount;


                    paidTO.RemainingAmount = paidTO.RemainingAmount + trans.Result.Amount;
                }
                _context.Update(paidFrom);
                _context.Update(paidTO);
                _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AddTransaction(Guid UserId, int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_AddSettleUpTransactionDetail")
               .WithSqlParams(
               (nameof(UserId), UserId),
               (nameof(GroupId), GroupId))
               .ExecuteStoredProcedureAsync<SP_GetAllExpensesDetailsAccGroupId>();
            return true;
        }
        public async Task<bool> AddTransactionbyCsharp(int groupId,JsonElement expense)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(expense);
            SP_GetSettleUpPaymentData expenseDTO = JsonConvert.DeserializeObject<SP_GetSettleUpPaymentData>(json);

            Summary summary = new Summary();
            var paidFrom = _context.Summaries.First(x => x.GroupId == groupId && x.IsActive == true && x.ParticipantId == expenseDTO.PaidById);
            var paidTO = _context.Summaries.First(x => x.GroupId == groupId && x.IsActive == true && x.ParticipantId == expenseDTO.PaidToId);

            //if (expenseDTO.Amount>0)
            //{
            //    paidFrom.RemainingAmount = paidFrom.RemainingAmount + expenseDTO.Amount;


            //    paidTO.RemainingAmount = paidTO.RemainingAmount - expenseDTO.Amount;
            //}
            //else
            //{
            //    paidFrom.RemainingAmount = paidFrom.RemainingAmount - expenseDTO.Amount;


            //    paidTO.RemainingAmount = paidTO.RemainingAmount + expenseDTO.Amount;
            //}

            Transaction transaction = new Transaction();
            transaction.Amount = expenseDTO.Amount;
            transaction.PaidFromMemberId = paidFrom.ParticipantId;
            transaction.PaidToMemberId = paidTO.ParticipantId;
            transaction.AddedBy= paidFrom.ParticipantId;
            _context.Transactions.Add(transaction);
            //_context.Update(paidFrom);
            //_context.Update(paidTO);
            _context.SaveChanges();
            //var obj = _unitOfWork.Expense.GetAllExpensesAccGroupId(GroupId);
            //int paidBy = expenseDTO.PaidBy;
            //decimal totalAMount = expenseDTO.Amount;
            return true;
        }

        public async Task<SP_GetSettleUpPaymentData> GetSettleUpPaymentData(Guid UserId, int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_GetSettleUpPaymentData")
              .WithSqlParams(
              (nameof(UserId), UserId),
              (nameof(GroupId), GroupId))
              .ExecuteStoredProcedureAsync<SP_GetSettleUpPaymentData>();
            return obj.Result[0];
        }

        public async Task<IEnumerable<SP_GetAllTransactionDetailsAccGroupId>> GetTransactionDataAccGroupId(Guid UserId, int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_GetAllTransactionDetailsAccGroupId")
              .WithSqlParams(
              (nameof(UserId), UserId),
              (nameof(GroupId), GroupId))
              .ExecuteStoredProcedureAsync<SP_GetAllTransactionDetailsAccGroupId>();
            return obj.Result;
        }

        public async Task<IEnumerable<SP_GetExpenseSummaryDetailAccGroupId>> GetExpenseSummaryDetailAccGroupId(Guid UserId, int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_GetExpenseSummaryDetailAccGroupId")
              .WithSqlParams(
              (nameof(UserId), UserId),
              (nameof(GroupId), GroupId))
              .ExecuteStoredProcedureAsync<SP_GetExpenseSummaryDetailAccGroupId>();
            return obj.Result;
        }
    }
}
