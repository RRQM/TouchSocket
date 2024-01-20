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

using System;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc调用上下文
    /// </summary>
    public interface ICallContext : IResolverObject
    {
        /// <summary>
        /// 调用此服务的主体。
        /// <para>
        /// <list type="bullet">
        /// <item>当该服务在ITcpService及派生中调用时，该值一般为ISocketClient对象。</item>
        /// <item>当该服务在ITcpClient及派生中调用时，该值一般为ITcpClient对象。</item>
        /// </list>
        /// </para>
        /// </summary>
        object Caller { get; }

        /// <summary>
        /// 本次调用的<see cref="MethodInstance"/>
        /// </summary>
        MethodInstance MethodInstance { get; }

        /// <summary>
        /// 可取消的调用令箭
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// TokenSource
        /// </summary>
        [Obsolete("此配置已被弃用，请使用Token和Cancel()代替该功能。", true)]
        CancellationTokenSource TokenSource { get; }

        /// <summary>
        /// 执行取消<see cref="ICallContext.Token"/>。
        /// </summary>
        /// <returns></returns>
        public void Cancel();
    }
}