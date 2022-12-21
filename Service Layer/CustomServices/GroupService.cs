using DomainLayer.Data;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Icao;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class GroupService : Repository<Group>, IGroupService
    {
        private readonly RxSplitterContext _context;
        public GroupService(RxSplitterContext context) : base(context)
        {
            _context = context;
        }
        public override bool Delete(Group group)
        {
            try
            {
                group.UpdatedOn = DateTime.Now;
                group.IsDeleted = true;
                _context.Groups.Update(group);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<Group> GetGroupDataWithMembersByGroupId(int GroupId)
        {
            List<Group> a = _context.Groups.Where(x => x.Id == GroupId).Include(x=>x.GroupMembers).ToList();
            return a;
        }
    }
}
