using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using Repository_Layer.IRepository;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class ReportService : Repository<Report>, IReportService
    {
        private readonly RxSplitterContext _context;
        private readonly ISprocRepository _sprocRepo;
        private readonly IMapper _mapper;

        public ReportService(RxSplitterContext context, IMapper mapper, ISprocRepository sprocRepo) : base(context)
        {

            _context = context;
            _sprocRepo = sprocRepo;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SP_GetAllExpensesChart>> GetAllExpensesChart(Guid UserId)
        {
            var obj = _sprocRepo.GetStoredProcedure("sp_allExpensesChart")
               .WithSqlParams(("UserId", UserId.ToString()))
               .ExecuteStoredProcedureAsync<SP_GetAllExpensesChart>();
            return obj.Result;
        }
        public async Task<IEnumerable<SP_GetAllExpensesChart>> GetGroupExpensesChart(Guid UserId,int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("sp_allExpensesChart")
               .WithSqlParams(("UserId", UserId.ToString()),("groupId", GroupId))
               .ExecuteStoredProcedureAsync<SP_GetAllExpensesChart>();
            return obj.Result;
        }

        public async Task<IEnumerable<monthWiseChart>> GetMonthwiseExpenses(Guid UserId, int? year)
        {
            var list = new List<monthWiseChart>();
            //string[] Months =["January", "February", "March", "April", "May","June", "July", "August", "September", "October", "November", "December"];
            string[] months = new string[] {"January", "February", "March", "April", "May",
  "June", "July", "August", "September", "October", "November", "December"};

            for (int i = 1; i <= 12; i++)
            {
               var data = _sprocRepo.GetStoredProcedure("sp_MonthwiseExpenses")
              .WithSqlParams(("UserId", UserId.ToString()),("year",year),("month", i))
              .ExecuteStoredProcedureAsync<sp_MonthwiseExpenses>();

                list.Add(new monthWiseChart
                {
                    name = months[i-1],
                    series = data.Result.ToList()
                });

            }
            return list;

        }

    }
}
