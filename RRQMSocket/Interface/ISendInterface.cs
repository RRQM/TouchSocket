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
using RRQMCore;
using RRQMCore.ByteManager;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端发送接口
    /// </summary>
    public interface IClientSender : ISenderBase
    {
        /// <summary>
        /// 同步组合发送
        /// </summary>
        /// <param name="transferBytes"></param>
        void Send(IList<TransferByte> transferBytes);

        /// <summary>
        /// 异步组合发送
        /// </summary>
        /// <param name="transferBytes"></param>
        void SendAsync(IList<TransferByte> transferBytes);
    }

    /// <summary>
    /// 具有发送功能的接口
    /// </summary>
    public interface ISenderBase
    {
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

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void SendAsync(byte[] buffer, int offset, int length);

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void SendAsync(byte[] buffer);

        /// <summary>
        /// IOCP发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        void SendAsync(ByteBlock byteBlock);
    }

    /// <summary>
    /// 通过ID发送
    /// </summary>
    public interface IIDSender
    {
        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Send(string id, byte[] buffer, int offset, int length);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Send(string id, byte[] buffer);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Send(string id, ByteBlock byteBlock);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void SendAsync(string id, byte[] buffer, int offset, int length);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void SendAsync(string id, byte[] buffer);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="RRQMNotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void SendAsync(string id, ByteBlock byteBlock);
    }


    /// <summary>
    /// 发送等待接口
    /// </summary>
    public interface IWaitSender
    {
        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        byte[] SendThenReturn(byte[] buffer, int offset, int length, CancellationToken token = default);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        byte[] SendThenReturn(byte[] buffer, CancellationToken token = default);

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        byte[] SendThenReturn(ByteBlock byteBlock, CancellationToken token = default);

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, CancellationToken token = default);

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        Task<byte[]> SendThenReturnAsync(byte[] buffer, CancellationToken token = default);

        /// <summary>
        /// IOCP发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="token"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, CancellationToken token = default);
    }
}