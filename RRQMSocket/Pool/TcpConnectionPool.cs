//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace RRQMSocket
{
    /// <summary>
    /// 连接池
    /// </summary>
    public class TcpConnectionPool<T> : IConnectionPool<T> where T : TcpClient
    {
        private TcpConnectionPool()
        {
            this.Logger = new Log();
            this.ErrorClientList = new List<T>();
            this.queue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// 创建连接池
        /// </summary>
        /// <param name="capacity">容量</param>
        public static TcpConnectionPool<T> CreatConnectionPool(int capacity)
        {
            return CreatConnectionPool(capacity, new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20), null, null);
        }

        /// <summary>
        /// 创建连接池
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="onClientIniCallback">当每个连接单元被初始化时回调</param>
        /// <returns></returns>
        public static TcpConnectionPool<T> CreatConnectionPool(int capacity, Action<T> onClientIniCallback)
        {
            return CreatConnectionPool(capacity, new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20), onClientIniCallback, null);
        }

        /// <summary>
        /// 创建连接池
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="bytePool">指定内存池实例</param>
        /// <param name="onClientIniCallback">当每个连接单元被初始化时回调</param>
        /// <param name="args">创建单元时构造函数参数</param>
        /// <returns></returns>
        public static TcpConnectionPool<T> CreatConnectionPool(int capacity, BytePool bytePool, Action<T> onClientIniCallback, params object[] args)
        {
            if (capacity < 1)
            {
                throw new RRQMException("容量不可小于1");
            }

            TcpConnectionPool<T> connectionPool = new TcpConnectionPool<T>();
            connectionPool.BytePool = bytePool;
            connectionPool.Capacity = capacity;
            for (int i = 0; i < capacity; i++)
            {
                T client = (T)Activator.CreateInstance(typeof(T), args);
                connectionPool.queue.Enqueue(client);
                onClientIniCallback?.Invoke(client);
            }
            return connectionPool;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="setting"></param>
        /// <returns>连接成功数</returns>
        public int Connect(ConnectSetting setting)
        {
            IPAddress IP = IPAddress.Parse(setting.TargetIP);
            EndPoint endPoint = new IPEndPoint(IP, setting.TargetPort);
            return Connect(endPoint);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>连接成功数</returns>
        public int Connect(EndPoint endPoint)
        {
            int count = 0;
            int successCount = 0;
            while (count < this.Capacity)
            {
                T client;
                if (this.queue.TryDequeue(out client))
                {
                    try
                    {
                        client.Connect(endPoint);
                        successCount++;
                    }
                    finally
                    {
                        this.queue.Enqueue(client);
                    }
                }
                count++;
            }
            return successCount;
        }

        private ConcurrentQueue<T> queue;

        /// <summary>
        /// 当池中的客户都端发生错误时
        /// </summary>
        public event RRQMMessageEventHandler OnClientError;

        /// <summary>
        /// 发生错误的客户端列表
        /// </summary>
        public List<T> ErrorClientList { get; private set; }

        /// <summary>
        /// 对象池容量
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 可使用数量
        /// </summary>
        public int FreeSize { get { return this.queue.Count; } }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体
        /// </summary>
        /// <returns></returns>
        public T GetNextClient()
        {
            T client;
            this.queue.TryPeek(out client);
            return client;
        }

        /// <summary>
        /// 补充成员
        /// </summary>
        /// <param name="client"></param>
        public void Replenish(T client)
        {
            this.queue.Enqueue(client);
            this.Capacity++;
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(byte[] buffer, int offset, int length)
        {
            int count = 0;
            while (true)
            {
                T client;
                if (this.queue.TryDequeue(out client))
                {
                    try
                    {
                        client.Send(buffer, offset, length);
                        this.queue.Enqueue(client);
                        break;
                    }
                    catch (Exception ex)
                    {
                        this.ErrorClientList.Add(client);
                        Logger.Debug(LogType.Warning, client, ex.Message);
                        this.OnClientError?.Invoke(client, new MesEventArgs(ex.Message));
                    }
                }
                if (++count > this.FreeSize)
                {
                    throw new RRQMException();
                }
            }
        }

        /// <summary>
        /// 清空池中对象
        /// </summary>
        public void Clear()
        {
            T client;
            while (this.queue.TryDequeue(out client))
            {
                client.Dispose();
            }
            foreach (var item in this.ErrorClientList)
            {
                item.Dispose();
            }
            this.ErrorClientList.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }
    }
}