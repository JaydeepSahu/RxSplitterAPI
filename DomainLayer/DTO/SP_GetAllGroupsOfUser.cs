using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class SP_GetAllGroupsOfUser
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string AddedBy { get; set; }
        public string  AddedOn { get; set; }
    }
}
