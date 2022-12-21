using DomainLayer.Data;
using Repository_Layer.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface IGroupMemberService:IRepository<GroupMember>
    {
        //List<GroupMember> 
    }
}
