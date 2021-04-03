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
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// Socket接口
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// IP
        /// </summary>
        string IP { get; }

        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 获取连接的唯一标识
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 数据交互缓存池限制，Min:1k Byte，Max:10Mb Byte
        /// </summary>
        int BufferLength { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }

        /// <summary>
        /// 内存池实例
        /// </summary>
        BytePool BytePool { get; }
    }
}