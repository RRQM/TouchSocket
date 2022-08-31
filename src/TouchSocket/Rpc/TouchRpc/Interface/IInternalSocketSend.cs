//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 不检验任何协议
    /// </summary>
    internal interface IInternalRpc
    {
        #region Socket同步直发

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataBuffer"></param>
        void SocketSend(short protocol, byte[] dataBuffer);

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        void SocketSend(short protocol, ByteBlock byteBlock);

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        void SocketSend(short protocol);

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void SocketSend(short protocol, byte[] dataBuffer, int offset, int length);

        #endregion Socket同步直发

        #region Socket异步直发

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataBuffer"></param>
        void SocketSendAsync(short protocol, byte[] dataBuffer);

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        void SocketSendAsync(short protocol);

        /// <summary>
        /// 不检验任何协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void SocketSendAsync(short protocol, byte[] dataBuffer, int offset, int length);

        #endregion Socket异步直发

        /// <summary>
        /// 移除通道
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveChannel(int id);
    }
}