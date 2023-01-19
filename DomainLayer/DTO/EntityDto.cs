using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.DTO
{
    public class EntityDto:AuditableEntityDto
    {
        public int? Id { get; set; }
    }
}
