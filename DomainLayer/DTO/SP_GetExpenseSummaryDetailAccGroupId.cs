using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_GetExpenseSummaryDetailAccGroupId
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int ParticipantId { get; set; }
        public decimal RemainingAmount { get; set; }

    }
}
