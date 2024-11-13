using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    internal class WebApiParameterInfo
    {
        public WebApiParameterInfo(RpcParameter parameter)
        {
            this.IsFromBody = parameter.ParameterInfo.IsDefined(typeof(FromBodyAttribute), false);

            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromQueryAttribute), false) is FromQueryAttribute fromQueryAttribute)
            {
                this.IsFromQuery = true;
                this.FromQueryName = fromQueryAttribute.Name ?? parameter.Name;
            }
            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromFormAttribute), false) is FromFormAttribute fromFormAttribute)
            {
                this.IsFromForm = true;
                this.FromFormName = fromFormAttribute.Name ?? parameter.Name;
            }

            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromHeaderAttribute), false) is FromHeaderAttribute fromHeaderAttribute)
            {
                this.IsFromHeader = true;
                this.FromHeaderName = fromHeaderAttribute.Name ?? parameter.Name;
            }

            this.Parameter = parameter;
        }

        public string FromFormName { get; }
        public string FromHeaderName { get; }
        public string FromQueryName { get;  }
        public bool IsFromBody { get; }
        public bool IsFromForm { get; }
        public bool IsFromHeader { get;  }
        public bool IsFromQuery { get;  }
        public RpcParameter Parameter { get; }
    }
}
