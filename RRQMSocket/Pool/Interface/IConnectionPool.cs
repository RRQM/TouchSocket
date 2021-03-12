using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Pool;

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
        BytePool BytePool { get;}

        /// <summary>
        /// 对象池容量
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 发生错误的客户端列表
        /// </summary>
        List<T> ErrorClientList { get;}

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
