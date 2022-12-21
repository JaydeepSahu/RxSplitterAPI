using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Helper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<UserDetailDTO, UserDetail>().ReverseMap();
           
            //CreateMap<UserDetailDTO, UserDetail>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)).ForMember(dest => dest.Id, opt => opt.Ignore());
            //CreateMap<UserDetail, UserDetailDTO>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}