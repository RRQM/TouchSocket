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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 发送事件
    /// </summary>
    public class SendingEventArgs : TouchSocketEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public SendingEventArgs(byte[] buffer, int offset, int length)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Length = length;
            this.AddOperation(Operation.Permit);
        }

        /// <summary>
        /// 数据缓存区，该属性获取来自于内存池，所以最好不要引用该对象，可以同步使用该对象
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// 缓存偏移
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; }
    }
}
