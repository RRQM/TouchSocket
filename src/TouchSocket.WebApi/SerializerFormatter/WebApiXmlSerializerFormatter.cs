using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.WebApi
{
    sealed class WebApiXmlSerializerFormatter : XmlStringToClassSerializerFormatter<HttpContext>
    {
        public override bool TrySerialize(HttpContext state, in object target, out string source)
        {
            switch (state.Request.Accept)
            {
                case "application/json":
                case "text/json":
                    {
                        source = default;
                        return false;
                    }
                case "application/xml":
                case "text/xml":
                case "text/plain":
                default:
                    return base.TrySerialize(state, target, out source);
            }
        }
    }
}
