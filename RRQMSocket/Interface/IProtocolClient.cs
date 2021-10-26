using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 定制协议的终端接口
    /// </summary>
    public interface IProtocolClient : ITokenClient
    {
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        Channel CreateChannel();

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        void Send(short procotol, byte[] buffer);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Send(short procotol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        void Send(short procotol, ByteBlock dataByteBlock);

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="procotol"></param>
        void Send(short procotol);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        void SendAsync(short procotol, byte[] buffer);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void SendAsync(short procotol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        void SendAsync(short procotol, ByteBlock dataByteBlock);

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="procotol"></param>
        void SendAsync(short procotol);

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TrySubscribeChannel(int id, out Channel channel);
    }
}