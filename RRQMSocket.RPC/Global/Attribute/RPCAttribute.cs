//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC方法属性基类
    /// </summary>
    public abstract class RPCAttribute : Attribute
    {
        private MethodFlags methodFlags = MethodFlags.None;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodFlags"></param>
        public RPCAttribute(MethodFlags methodFlags)
        {
            this.methodFlags = methodFlags;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodFlags"></param>
        public RPCAttribute(string methodName, MethodFlags methodFlags)
        {
            this.MethodName = methodName;
            this.methodFlags = methodFlags;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodName"></param>
        public RPCAttribute(string methodName)
        {
            this.MethodName = methodName;
        }


        /// <summary>
        /// 异步执行。
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// 函数标识
        /// </summary>
        public MethodFlags MethodFlags
        {
            get { return methodFlags; }
            set { methodFlags=value; }
        }

        /// <summary>
        /// 函数名
        /// </summary>
        public string MethodName { get; set; }
    }
}