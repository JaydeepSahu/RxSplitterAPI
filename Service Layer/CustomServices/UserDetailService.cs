using DomainLayer.Data;
using DomainLayer.Common;
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
    public class UserDetailService : Repository<UserDetail>, IUserDetailService
    {
        private readonly RxSplitterContext _context;
        public UserDetailService(RxSplitterContext context) : base(context)
        {
            _context = context;
        }

        public UserDetail GetAuthenticatedUserDetail(Login obj)
        {
            var user=_context.UserDetails.Where(x => x.Email.ToLower() == obj.UserName.ToLower() && x.Password == CommonMethods.Encryptword(obj.Password) && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            //return _mapper.Map<UserDetail>(user);
            return user;
        }

        public bool ResetPassword(Guid Id, string NewPassword)
        {
            try
            {
                var user = _context.UserDetails.Where(x => x.Id == Id).FirstOrDefault();
                user.UpdatedOn=DateTime.Now;
                user.Password= NewPassword;
                _context.UserDetails.Update(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override bool Delete(UserDetail user)
        {
            try
            {
                user.UpdatedOn= DateTime.Now;
                user.IsDeleted = true;
                _context.UserDetails.Update(user);
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
