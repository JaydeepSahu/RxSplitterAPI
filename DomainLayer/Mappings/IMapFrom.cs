using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Mappings
{
    public  interface IMapFrom
    {
        void Mapping(Profile profile);
    }
}
