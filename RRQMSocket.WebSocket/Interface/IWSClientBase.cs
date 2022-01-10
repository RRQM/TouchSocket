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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket终端基类接口
    /// </summary>
    public interface IWSClientBase:ITcpClientBase,ISendBase
    {
        /// <summary>
        /// WebSocket版本号。
        /// </summary>
        string WebSocketVersion { get; set; }

        /// <summary>
        /// 通过WebSocket协议发送文本。
        /// </summary>
        /// <param name="text"></param>
        void Send(string text);

        /// <summary>
        /// 通过WebSocket协议发送文本。
        /// </summary>
        /// <param name="text"></param>
        void SendAsync(string text);

        /// <summary>
        /// 发送WebSocket数据帧
        /// </summary>
        /// <param name="dataFrame"></param>
        void Send(WSDataFrame dataFrame);

        /// <summary>
        /// 发送WebSocket数据帧
        /// </summary>
        /// <param name="dataFrame"></param>
        void SendAsync(WSDataFrame dataFrame);

        #region 同步分包发送

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="packageSize"></param>
        void SubpackageSend(byte[] buffer, int offset, int length, int packageSize);

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packageSize"></param>
        void SubpackageSend(byte[] buffer, int packageSize);

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="packageSize"></param>
        void SubpackageSend(ByteBlock byteBlock, int packageSize);
        #endregion

        #region 异步分包发送

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="packageSize"></param>
        void SubpackageSendAsync(byte[] buffer, int offset, int length, int packageSize);

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packageSize"></param>
        void SubpackageSendAsync(byte[] buffer, int packageSize);

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="packageSize"></param>
        void SubpackageSendAsync(ByteBlock byteBlock, int packageSize);
        #endregion
    }
}
