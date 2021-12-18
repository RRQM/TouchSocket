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

namespace RRQMSocket
{
    /// <summary>
    /// 接收类型
    /// </summary>
    public enum ReceiveType : byte
    {
        /// <summary>
        /// 完成端口
        /// </summary>
        IOCP,

        /// <summary>
        /// 独立线程阻塞
        /// </summary>
        BIO,

        /// <summary>
        /// 网络流模式，接收、发送方式由IOCP切换为NetworkStream.Read/Write
        /// 同时服务器超时清理将变得不可用，且适配也将失效。
        /// </summary>
        NetworkStream
    }
}