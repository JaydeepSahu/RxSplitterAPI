using AutoMapper;
using DomainLayer.Data;

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class UserDetailDTO : AuditableEntityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
      
        public string Email { get; set; } = null!;
         
        public string Password { get; set; } = null!;
        
        public bool? IsActive { get; set; }

        public bool IsDeleted { get; set; }
        

    }
}
