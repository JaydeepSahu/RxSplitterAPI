using AutoMapper;
using DomainLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class GroupMemberDTO:EntityDto
    {
        public string Email { get; set; } = null!;
        public int GroupId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<GroupMember, GroupMemberDTO>().ReverseMap();
        }
    }
}
