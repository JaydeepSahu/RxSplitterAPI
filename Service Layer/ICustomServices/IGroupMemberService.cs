using DomainLayer.Data;
using DomainLayer.DTO;
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
        Task<IEnumerable<GroupMemberDTO>> GetAllMembersByGroupId(int GroupId);
        Task<int> DeleteMemberByGroupId(int memberId, int GroupId);
    }
}
