using AutoMapper;
using DomainLayer.Data;
using Repository_Layer.IRepository;
using Repository_Layer.Repository;
using Service_Layer.ICustomServices;
using Service_Layer.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class MemberInvitationService : Repository<MemberInvitation>, IMemberInvitationService
    {
        private readonly RxSplitterContext _context;
        private readonly ISprocRepository _sprocRepo;
        private readonly IMapper _mapper;

        public MemberInvitationService(RxSplitterContext context, IMapper mapper, ISprocRepository sprocRepo) : base(context)
        {

            _context = context;
            _sprocRepo = sprocRepo;
            _mapper = mapper;
        }

        public async Task<bool> AccectInvitation(GroupMember mbr)
        {
            try
            {
                var invite = _context.MemberInvitations.Where(x => x.MemberId == mbr.Id && x.GroupId == mbr.GroupId).FirstOrDefault();

                //if (invite.InvitationStatus == "0")
                //{
                    invite.UpdatedOn = DateTime.UtcNow;
                    invite.InvitationStatus = "1";
                    _context.MemberInvitations.Update(invite);
                    _context.SaveChanges();

                   
                //}
                //else
                //{
                //    return false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeclineInvitation(GroupMember mbr)
        {
            try
            {
                var invite = _context.MemberInvitations.Where(x => x.MemberId == mbr.Id && x.GroupId == mbr.GroupId).FirstOrDefault();
                //if (invite.InvitationStatus == "0")
                //{
                    invite.UpdatedOn = DateTime.UtcNow;
                    invite.InvitationStatus = "-1";
                    _context.MemberInvitations.Update(invite);
                    _context.SaveChanges();
                //}
                //else
                //{
                //    return false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> InsertInvitationDetail(int memberId, int GroupId, Guid UserId)
        {
            try
            {
                MemberInvitation obj = new MemberInvitation();
                obj.TokenGeneratedByUser = UserId;
                obj.GroupId = GroupId;
                obj.MemberId = memberId;
                obj.InvitationStatus = "0";
                _context.MemberInvitations.Add(obj);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsMemberInvitationExist(int memberId, int GroupId)
        {
            return _context.MemberInvitations.Any(x => x.MemberId == memberId && x.GroupId == GroupId && x.InvitationStatus == "0");
        }

    }
}
