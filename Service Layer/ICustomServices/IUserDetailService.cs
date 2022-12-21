using DomainLayer.Data;
using DomainLayer.Common;
using Repository_Layer.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface IUserDetailService:IRepository<UserDetail>
    {
        UserDetail GetAuthenticatedUserDetail(Login obj);
        bool ResetPassword(Guid Id, string Password);

    }
}
