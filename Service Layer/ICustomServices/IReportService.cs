using DomainLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface IReportService
    {
        Task<IEnumerable<SP_GetAllExpensesChart>> GetAllExpensesChart(Guid UserId);
        Task<IEnumerable<SP_GetAllExpensesChart>> GetGroupExpensesChart(Guid UserId, int groupId);
        Task<IEnumerable<monthWiseChart>> GetMonthwiseExpenses(Guid UserId, int? year);

    }
}
