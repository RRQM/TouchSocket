//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC方法标记属性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RRQMRPCMethodAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMRPCMethodAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodKey">指定函数键</param>
        public RRQMRPCMethodAttribute(string methodKey)
        {
            this.MethodKey = methodKey;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodKey">指定函数键</param>
        /// <param name="async">异步执行该方法</param>
        public RRQMRPCMethodAttribute(string methodKey, bool async)
        {
            this.MethodKey = methodKey;
            this.Async = async;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="async">异步执行该方法</param>
        public RRQMRPCMethodAttribute(bool async)
        {
            this.Async = async;
        }

        /// <summary>
        /// 是否异步执行
        /// </summary>
        public bool Async { get; private set; }

        /// <summary>
        /// 注册键
        /// </summary>
        public string MethodKey { get; private set; }
    }
}