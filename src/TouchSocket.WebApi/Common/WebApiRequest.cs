using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.WebApi
{
    public class WebApiRequest
    {
        public HttpMethodType Method { get; set; }
        public object Body { get; set; }
        public string ContentType { get; set; }
        public KeyValuePair<string,string>[] Headers { get; set; }
        public KeyValuePair<string,string>[] Querys { get; set; }
        public KeyValuePair<string,string>[] Forms { get; set; }
    }
}
