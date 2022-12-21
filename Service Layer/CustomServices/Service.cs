using AutoMapper;
using DomainLayer.Common;
using DomainLayer.DTO;
using Repository_Layer.IRepository;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.CustomServices
{
    public class Service<T, TDto> : IService<T, TDto>
        where TDto : EntityDto where T : Entity
    {
        private readonly IRepository<T> _repository;
        private readonly IMapper _mapper;


        public Service(IRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}
