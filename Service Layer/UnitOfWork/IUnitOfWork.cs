using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserDetailService User { get; }
        IGroupService Group { get; }
        IGroupMemberService GroupMember { get; }


        void Save();
        
    }
}
