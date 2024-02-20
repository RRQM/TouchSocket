using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.WebApi
{
    sealed class WebApiJsonSerializerFormatter : JsonStringToClassSerializerFormatter<HttpContext>
    {
        public override bool TrySerialize(HttpContext state, in object target, out string source)
        {
            switch (state.Request.Accept)
                {
                case "application/xml":
                case "text/xml":
                    {
                        source = default;
                        return false;
                    }
                case "application/json":
                case "text/json":
                case "text/plain":
                default:
                    return base.TrySerialize(state, target, out source);
            }
        }
    }
}
