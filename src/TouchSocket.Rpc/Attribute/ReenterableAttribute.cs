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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识一个方法或类是否可重新进入。
    /// </summary>
    /// <remarks>
    /// 该属性主要用于并发控制和同步机制中，指示被标记的方法或类是否可以在尚未完成执行时再次被调用。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]

    public sealed class ReenterableAttribute : Attribute
    {
        /// <summary>
        /// 初始化 ReenterableAttribute 类的新实例。
        /// </summary>
        /// <param name="reenterable">指示是否可重新进入。true 表示可重新进入；false 表示不可重新进入。</param>
        public ReenterableAttribute(bool reenterable)
        {
            this.Reenterable = reenterable;
        }

        /// <summary>
        /// 获取一个值，指示是否可重新进入。
        /// </summary>
        public bool Reenterable { get; }
    }
}
