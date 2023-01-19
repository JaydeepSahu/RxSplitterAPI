using AutoMapper;
using DomainLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class GroupMemberDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } 
        public int GroupId { get; set; }
        public Guid UserId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Icon { get; set; }
        
    }
}
