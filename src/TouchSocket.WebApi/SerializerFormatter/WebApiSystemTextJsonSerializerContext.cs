#if SystemTextJson
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TouchSocket.WebApi
{
    [JsonSerializable(typeof(ActionResult))]
    public partial class WebApiSystemTextJsonSerializerContext: JsonSerializerContext
    {
    }
}
#endif

