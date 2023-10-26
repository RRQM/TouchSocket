//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Newtonsoft.Json;
using TouchSocket.Core;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcWaitResult
    /// </summary>
    public class JsonRpcWaitResult : JsonRpcResponseBase, IWaitResult
    {
        /// <summary>
        /// Result
        /// </summary>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonPropertyName("result")]
#endif

        [JsonProperty("result")]
        public object Result { get; set; }

        /// <summary>
        /// Error
        /// </summary>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonPropertyName("error")]
#endif

        [JsonProperty("error")]
        public JsonRpcError Error { get; set; }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif

        [JsonIgnore]
        public string Message { get; set; }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif

        [JsonIgnore]
        public long Sign { get => (long)this.Id; set => this.Id = value; }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif

        [JsonIgnore]
        public byte Status { get; set; }
    }
}