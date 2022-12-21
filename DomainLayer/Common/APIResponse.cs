using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Common
{
    public class APIResponse
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }
        public dynamic Response { get; set; }

    }
}
