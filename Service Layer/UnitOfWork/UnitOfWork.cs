using DomainLayer.Common;
using DomainLayer.Data;
using Service_Layer.CustomServices;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserDetailService User { get;private set; }
        public IGroupService Group { get; private set; }
        public IGroupMemberService GroupMember { get; private set; }



        private RxSplitterContext _context;
        public UnitOfWork(RxSplitterContext context)
        {
            _context = context;
            User = new UserDetailService(context);
            Group = new GroupService(context);
            GroupMember = new GroupMemberService(context);


        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
