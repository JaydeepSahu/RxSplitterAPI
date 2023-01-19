using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_GetExpenseTransactionDetailAccExpenseId
    {
        //Id	TransactionNumber	PaidByMemberId	PaidByMemberName	PaidByMemberEmail	ParticipantMemberId	ParticipantMemberName	ParticipantMemberEmail	DistributedAmount
        public int Id { get; set; }
        public string TransactionNumber { get; set; }
        public int PaidByMemberId { get; set; }
        public string PaidByMemberName { get; set; }
        public string PaidByMemberEmail { get; set; }
        public int ParticipantMemberId { get; set; }
        public string ParticipantMemberName { get; set; }
        public string ParticipantMemberEmail { get; set; }
        public decimal DistributedAmount { get; set; }

    }
}
