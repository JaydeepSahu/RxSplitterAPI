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
    public interface IExpenseService : IRepository<Expense>
    {
        Task<ExpensesDTO> SaveExpenseData(ExpensesDTO expense);

        Task<IEnumerable<dynamic>> GetAllExpensesAccGroupId(int GroupId, Guid UserId, string Mode);
        Task<IEnumerable<dynamic>> GetAllExpensesAccUserId(Guid UserId, string Mode);
        Task <int> AddSummary(JsonElement expense);
        Task<IEnumerable<Summary>> GetSummaryByGroupID(int GroupId);
        Task<int>AddInitialSummary(Summary summary);
        int UpdateSummary(int memberId, int GroupId);
        Task<IList<SP_totalGroupOweAmount>> GetUserDashboardDetails(Guid UserId);


    }
}
