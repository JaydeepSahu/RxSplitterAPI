using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.DTO
{
    public class AuditableEntityDto:EntityDto
    {
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
