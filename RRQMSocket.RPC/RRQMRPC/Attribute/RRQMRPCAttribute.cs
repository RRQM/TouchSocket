//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC方法标记属性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RRQMRPCAttribute : RPCAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMRPCAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName">指定键</param>
        public RRQMRPCAttribute(string methodName) : this(methodName, MethodFlags.None)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodFlags"></param>
        public RRQMRPCAttribute(MethodFlags methodFlags) : this(null, methodFlags)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="async"></param>
        public RRQMRPCAttribute(bool async) : this(null, MethodFlags.None, async)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodFlags"></param>
        /// <param name="async"></param>
        public RRQMRPCAttribute(MethodFlags methodFlags, bool async) : this(null, methodFlags, async)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodFlags"></param>
        public RRQMRPCAttribute(string methodName, MethodFlags methodFlags) : base(methodFlags)
        {
            this.MethodName = methodName;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodFlags"></param>
        /// <param name="async"></param>
        public RRQMRPCAttribute(string methodName, MethodFlags methodFlags, bool async) : base(methodName,methodFlags)
        {
            this.Async = async;
        }
    }
}