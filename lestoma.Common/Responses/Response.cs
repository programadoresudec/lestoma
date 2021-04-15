using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.Common.Responses
{
    public class Response
    {
        public string Mensaje { get; set; }
        public bool IsExito { get; set; }
        public object Data { get; set; }
    }
}
