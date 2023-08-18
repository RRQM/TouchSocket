using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcError
    /// </summary>
    public class JsonRpcError
    {
        /// <summary>
        /// code
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
