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
namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 流信息
    /// </summary>
    public struct StreamInfo
    {
        private readonly long size;

        private readonly string streamType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="size"></param>
        /// <param name="streamType"></param>
        public StreamInfo(long size, string streamType)
        {
            this.size = size;
            this.streamType = streamType;
        }

        /// <summary>
        /// 流长度
        /// </summary>
        public long Size => this.size;

        /// <summary>
        /// 流类型
        /// </summary>
        public string StreamType => this.streamType;
    }
}