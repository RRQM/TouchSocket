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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 服务器接口
    /// </summary>
    public interface IServiceBase : ISetupConfigObject
    {
        /// <summary>
        /// 名称
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// 服务器状态
        /// </summary>
        ServerState ServerState { get; }

        /// <summary>
        /// 异步启动
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        Task StartAsync();

        /// <summary>
        /// 异步停止
        /// </summary>
        /// <exception cref="Exception"></exception>
        Task StopAsync();
    }
}