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
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器接口
    /// </summary>
    public interface IService:IDisposable
    {
        /// <summary>
        /// 获取默认内存池实例
        /// </summary>
        BytePool BytePool { get;}

        /// <summary>
        /// 服务器状态
        /// </summary>
        ServerState ServerState { get; }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="serverConfig">配置</param>
        /// <exception cref="RRQMException"></exception>
        void Setup(IServerConfig serverConfig);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="port"></param>
        /// <exception cref="RRQMException"></exception>
        void Setup(int port);

        /// <summary>
        /// 启动
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        void Start();

        /// <summary>
        /// 停止
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        void Stop();

    }
}