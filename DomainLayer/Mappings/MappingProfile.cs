using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DomainLayer.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GroupMember, GroupMemberDTO>().ReverseMap();
            //ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }
        //private void ApplyMappingsFromAssembly(Assembly assembly)
        //{
        //    var types = assembly.GetExportedTypes()
        //         .Where(x => typeof(IMapFrom).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
        //        .ToList();

        //    foreach (var type in types)
        //    {
        //        var instance = Activator.CreateInstance(type);
        //        var methodInfo = type.GetMethod("Mapping");
        //        methodInfo?.Invoke(instance, new object[] { this });
        //    }
        //}
    }
}
