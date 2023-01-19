using DomainLayer.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class GroupDTO:EntityDto
    {
        public string? GroupName { get; set; }
        public IFormFile? GroupImage { get; set; }
        public string? GroupImagePath { get; set; }
        public List<GroupMember> lstMember { get; set; }
        public Guid AddedBy { get; set; }
    }
}
