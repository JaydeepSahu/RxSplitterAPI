using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_GetAllTransactionDetailsAccGroupId
    {
        //Id Amount  AddedOn Status  PaidFrom PaidTo IsLoggedIn

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string AddedOn { get; set; }
        public string Status { get; set; }
        public string PaidFrom { get; set; }
        public string PaidTo { get; set; }
        public string IsLoggedIn { get; set; }



    }
}
