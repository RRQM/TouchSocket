using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.WebApi
{
    public abstract class WebApiNameAttribute:Attribute
    {
        public string Name { get; set; }
    }
}
