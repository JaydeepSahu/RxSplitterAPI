using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_totalGroupOweAmount
    {
        public decimal PositiveSum { get; set; }
        public decimal NegativeSum { get; set; }
        public decimal RemainingAmount { get; set; }

    }
}
