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

using System.Collections.Generic;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp服务器接口
    /// </summary>
    public interface ITcpServiceBase : IConnectableService
    {
        /// <summary>
        /// 网络监听集合
        /// </summary>
        IEnumerable<TcpNetworkMonitor> Monitors { get; }

        /// <summary>
        /// 添加一个地址监听。支持在服务器运行过程中动态添加。
        /// </summary>
        /// <param name="options"></param>
        void AddListen(TcpListenOption options);

        /// <summary>
        /// 移除一个地址监听。支持在服务器运行过程中动态移除。
        /// </summary>
        /// <param name="monitor">监听器</param>
        /// <returns>返回是否已成功移除</returns>
        bool RemoveListen(TcpNetworkMonitor monitor);
    }
}