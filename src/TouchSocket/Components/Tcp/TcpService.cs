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

namespace TouchSocket.Sockets
{

    /// <summary>
    /// Tcp服务类，继承自<see cref="TcpService{TClient}"/>，实现<see cref="ITcpService"/>接口。
    /// 该类用于提供基于TCP协议的服务。
    /// </summary>
    public class TcpService : TcpService<TcpSessionClient>, ITcpService
    {
        /// <summary>
        /// 创建新的客户端会话。
        /// </summary>
        /// <returns>返回一个新的PrivateTcpSessionClient对象。</returns>
        /// <remarks>
        /// 此方法覆盖了基类中的同名方法，用于生成自定义的TcpSessionClient实例。
        /// </remarks>
        protected sealed override TcpSessionClient NewClient()
        {
            return new PrivateTcpSessionClient();
        }

        /// <summary>
        /// 私有TcpSessionClient类，继承自TcpSessionClient。
        /// 该类用于提供自定义的TcpSessionClient实现。
        /// </summary>
        private sealed class PrivateTcpSessionClient : TcpSessionClient
        {
            
        }
    }
}