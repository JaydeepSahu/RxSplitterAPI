using DomainLayer.Data;
using DomainLayer.DTO;
using Repository_Layer.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface ITransactionService : IRepository<Transaction>
    {
        Task<bool> AddTransaction(Guid UserId, int GroupId);
        Task<SP_GetSettleUpPaymentData> GetSettleUpPaymentData(Guid UserId, int GroupId);
        Task<bool> AddTransactionbyCsharp(int groupId,JsonElement expense);
        Task<IEnumerable<SP_GetAllTransactionDetailsAccGroupId>> GetTransactionDataAccGroupId(Guid UserId, int GroupId);
        Task<bool> AcceptTransaction(Guid UserId, int TransactionId);

        Task<IEnumerable<SP_GetExpenseSummaryDetailAccGroupId>> GetExpenseSummaryDetailAccGroupId(Guid UserId, int GroupId);

    }
}
