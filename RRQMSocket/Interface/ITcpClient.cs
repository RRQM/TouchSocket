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
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端接口
    /// </summary>
    public interface ITcpClient : IClient
    {
        /// <summary>
        /// 判断是否在线
        /// </summary>
        bool Online { get; }

        /// <summary>
        /// IP及端口号
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        DataHandlingAdapter DataHandlingAdapter { get; }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        void Shutdown(SocketShutdown how);
    }
}