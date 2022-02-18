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

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器辅助接口
    /// </summary>
    public interface ITcpServiceBase : IService
    {
        /// <summary>
        /// 使用Ssl加密
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        SocketClientCollection SocketClients { get; }

        /// <summary>
        /// 网络监听集合
        /// </summary>
        NetworkMonitor[] Monitors { get; }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        void ResetID(WaitSetID waitSetID);

        /// <summary>
        /// 获取当前在线的所有ID集合
        /// </summary>
        /// <returns></returns>
        string[] GetIDs();

        /// <summary>
        /// 清理当前已连接的所有客户端
        /// </summary>
        void Clear();
    }
}