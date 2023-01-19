using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_GetSettleUpPaymentData
    {
        //PaidById PaidByName  PaidByEmail PaidToId    PaidToName PaidToEmail PaidToAmount Amount  IsNegative
        public int PaidById { get; set; }
        public string PaidByName { get; set; }
        public string PaidByEmail { get; set; }
        public int PaidToId { get; set; }
        public string PaidToName { get; set; }
        public string PaidToEmail { get; set; }
        public decimal PaidToAmount { get; set; }
        public decimal Amount { get; set; }
        public int IsNegative { get; set; }
    }
}
