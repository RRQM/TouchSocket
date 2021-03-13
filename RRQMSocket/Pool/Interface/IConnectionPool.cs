//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Pool;
using System.Collections.Generic;

namespace RRQMSocket.Pool
{
    /// <summary>
    /// 连接池接口
    /// </summary>
    public interface IConnectionPool<T> : IObjectPool where T : IUserClient
    {
        /// <summary>
        /// 当池中的客户都端发生错误时
        /// </summary>
        event RRQMMessageEventHandler OnClientError;

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 对象池容量
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 发生错误的客户端列表
        /// </summary>
        List<T> ErrorClientList { get; }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体
        /// </summary>
        /// <returns></returns>
        T GetNextClient();

        /// <summary>
        /// 补充成员
        /// </summary>
        /// <param name="client"></param>
        void Replenish(T client);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void Send(byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void Send(byte[] buffer);

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void Send(ByteBlock byteBlock);
    }
}