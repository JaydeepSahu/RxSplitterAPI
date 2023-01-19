using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class ExpenseTransactionDTO
    {
        public int ExpensesId { get; set; }
        public string TransactionNumber { get; set; }
        public int PaidByMemberId { get; set; }

        public int ParticipantMemberId { get; set; }

        public decimal? Amount { get; set; }
    }
}
