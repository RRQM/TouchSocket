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
using RRQMCore.ByteManager;
using RRQMCore.Log;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 客户端RPC接口
    /// </summary>
    public interface IRRQMRPCClient : IRPCClient
    {
        /// <summary>
        /// 获取ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        SerializeConverter SerializeConverter { get; }

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        RPCProxyInfo GetProxyInfo();

        /// <summary>
        /// 服务发现完成后
        /// </summary>
        event RRQMMessageEventHandler ServiceDiscovered;

        /// <summary>
        /// 发现服务
        /// </summary>
        /// <param name="isTrigger">是否触发初始化事件</param>
        /// <returns>已发现的服务</returns>
        MethodItem[] DiscoveryService(bool isTrigger=true);
    }
}