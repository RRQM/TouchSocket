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

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// 适用于WebApi的路由标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RouteAttribute : RPCMethodAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RouteAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="template"></param>
        public RouteAttribute(string template)
        {
            this.Template = template;
        }

        /// <summary>
        /// 路由模板
        /// </summary>
        public string Template { get; private set; }
    }
}