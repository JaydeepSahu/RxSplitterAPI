using DomainLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class ExpensesDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string ExpenseNumber { get; set; }


        public int PaidBy { get; set; }

        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }


        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string? FilePath { get; set; }
        public List<ExpenseTransactionDTO> lstExpenseTransaction { get; set; }
        public List<dynamic> lstTransaction { get; set; }
        public string Status { get; set; }

        public Guid? AddedBy { get; set; }
        public int? PaidByMemberId { get; set; }
        public string? PaidByMemberEmail { get; set; }
        public string? PaidByMemberName { get; set; }
        public int? LentById { get; set; }
        public int? LentByAmount { get; set; }
        public string? LentByName { get; set; }
        public string? LentByEmail { get; set; }

    }
}
