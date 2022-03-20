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

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 代理类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class RRQMProxyAttribute : Attribute
    {
        private string className;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="className"></param>
        public RRQMProxyAttribute(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException($"“{nameof(className)}”不能为 null 或空。", nameof(className));
            }

            this.className = className;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMProxyAttribute()
        {

        }

        /// <summary>
        /// 代理类名
        /// </summary>
        public string ClassName
        {
            get => this.className;
            set => this.className = value;
        }

    }
}
