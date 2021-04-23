//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 若汝棋茗内置TCP验证客户端
    /// </summary>
    public class RRQMTokenTcpClient<Tobj> : TokenTcpClient<Tobj>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMTokenTcpClient()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">设置内存池实例</param>
        public RRQMTokenTcpClient(BytePool bytePool) : base(bytePool)
        {
        }

        /// <summary>
        /// 当收到数据时
        /// </summary>
        public event RRQMByteBlockEventHandler OnReceivedData;

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, Tobj obj)
        {
            OnReceivedData?.Invoke(this, byteBlock);
        }
    }
}