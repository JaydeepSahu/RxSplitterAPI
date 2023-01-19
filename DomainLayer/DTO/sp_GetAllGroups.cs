using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class sp_GetAllGroups
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string AddedBy { get; set; }
        public string AddedOn { get; set; }
        public decimal Amount { get; set; }
        public int MemberCount { get; set; }
        public bool isActive { get; set; }
        public string Icon { get; set; }

    }
}
