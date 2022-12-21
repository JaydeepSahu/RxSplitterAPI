using DomainLayer.Data;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class GroupMemberService : Repository<GroupMember>, IGroupMemberService
    {
        private readonly RxSplitterContext _context;
        public GroupMemberService(RxSplitterContext context) : base(context)
        {
            _context = context;
        }
        public override bool Delete(GroupMember groupMember)
        {
            try
            {
                groupMember.UpdatedOn = DateTime.Now;
                groupMember.IsDeleted = true;
                _context.GroupMembers.Update(groupMember);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
