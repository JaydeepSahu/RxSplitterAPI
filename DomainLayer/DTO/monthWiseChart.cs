using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class monthWiseChart
    {
        public string name { get; set; }
        public List<sp_MonthwiseExpenses> series { get; set;}
    }
}
