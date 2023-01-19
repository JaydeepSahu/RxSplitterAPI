using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Repository_Layer.IRepository;
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
        private readonly ISprocRepository _sprocRepo;
        public GroupMemberService(RxSplitterContext context, ISprocRepository sprocRepo) : base(context)
        {
            _context = context;
            _sprocRepo = sprocRepo;
        }
        public override bool Delete(GroupMember groupMember)
        {
            try
            {
                groupMember.UpdatedOn = DateTime.UtcNow;
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

        public async Task<IEnumerable<GroupMemberDTO>> GetAllMembersByGroupId(int GroupId)
        {
            var obj = _sprocRepo.GetStoredProcedure("SP_GetAllMembersByGroupId")
                .WithSqlParams((nameof(GroupId), GroupId))
                .ExecuteStoredProcedureAsync<GroupMemberDTO>();
            return obj.Result;
        }

        public async Task<int> DeleteMemberByGroupId(int memberId, int GroupId)
        {
            var obj = _context.GroupMembers.Include(x => x.MemberInvitations).First(x => x.Id == memberId && x.GroupId == GroupId);
            obj.IsActive = false;
            obj.IsDeleted = true;
            _context.GroupMembers.Update(obj);

            var existingrecord = obj.MemberInvitations.FirstOrDefault(x => x.MemberId == memberId);
            if(existingrecord != null)
            {
                _context.MemberInvitations.Remove(existingrecord);
            }

            var updateisdel = _context.Summaries.FirstOrDefault(x => x.ParticipantId == memberId && x.GroupId == GroupId && x.IsActive == true);
            updateisdel.IsDelete = true;
            updateisdel.IsActive = false;
            _context.Summaries.Update(updateisdel);

            return _context.SaveChanges();

        }
    }
}
