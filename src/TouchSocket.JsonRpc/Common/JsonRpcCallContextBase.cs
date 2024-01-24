//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpc调用上下文
    /// </summary>
    public abstract class JsonRpcCallContextBase :CallContext, IJsonRpcCallContext
    {
        /// <summary>
        ///  JsonRpc调用上下文
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="jsonString"></param>
        public JsonRpcCallContextBase(object caller, string jsonString)
        {
            this.Caller = caller;
            this.JsonString = jsonString;
        }

        /// <summary>
        /// JsonRpc上下文
        /// </summary>
        public JsonRpcRequestContext JsonRpcContext { get; internal set; }

        /// <summary>
        /// Json字符串
        /// </summary>
        public string JsonString { get; }

        internal void SetMethodInstance(MethodInstance methodInstance)
        {
            this.MethodInstance = methodInstance;
        }
    }
}