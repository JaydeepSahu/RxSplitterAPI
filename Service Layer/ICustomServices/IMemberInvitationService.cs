using DomainLayer.Data;
using Repository_Layer.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface IMemberInvitationService : IRepository<MemberInvitation>
    {
        Task<bool> InsertInvitationDetail(int memberId, int GroupId, Guid UserId);

        Task<bool> AccectInvitation(GroupMember mbr);

        Task<bool> DeclineInvitation(GroupMember mbr);
        bool IsMemberInvitationExist(int memberId, int GroupId);
    }
}
