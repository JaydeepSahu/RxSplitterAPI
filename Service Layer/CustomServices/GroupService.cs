using DomainLayer.Data;
using DomainLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Icao;
using Repository_Layer.IRepository;
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
        private readonly ISprocRepository _sprocRepo;
        public GroupService(RxSplitterContext context, ISprocRepository sprocRepo) : base(context)
        {
            _context = context;
            _sprocRepo = sprocRepo;
        }
        public override bool Delete(Group group)
        {
            try
            {
                group.UpdatedOn = DateTime.UtcNow;
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

        public async Task<List<Group>> GetGroupDataWithMembersByGroupId(int GroupId)
        {
            var a = await _context.Groups.Where(x => x.Id == GroupId).Include(x=>x.GroupMembers).ToListAsync();
            return a;
        }

        public async Task<IEnumerable<SP_GetAllGroupsOfUser>> GetAllDetailGroupsOfUser(Guid UserId)
        {
            var obj =  _sprocRepo.GetStoredProcedure("SP_GetAllGroupsOfUser")
                .WithSqlParams(("UserId",UserId.ToString()))
                .ExecuteStoredProcedureAsync<SP_GetAllGroupsOfUser>();
            return obj.Result;
        }

        public async Task<IEnumerable<sp_GetAllGroups>> GetAllDetailGroups(Guid UserId)
        {
            var obj = _sprocRepo.GetStoredProcedure("sp_GETAllGroups")
                .WithSqlParams(("UserId", UserId.ToString()))
                .ExecuteStoredProcedureAsync<sp_GetAllGroups>();
            return obj.Result;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return _context.Categories.ToList();
        }

        public async Task<List<Currency>> GetAllCurrency()
        {
            return _context.Currencies.ToList();
        }
    }
}
