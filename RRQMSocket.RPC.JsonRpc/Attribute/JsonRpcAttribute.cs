//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// 适用于XmlRpc的标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class JsonRpcAttribute : RPCMethodAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodKey"></param>
        public JsonRpcAttribute(string methodKey)
        {
            this.MethodKey = methodKey;
        }

        /// <summary>
        /// 服务唯一标识
        /// </summary>
        public string MethodKey { get; private set; }
    }
}