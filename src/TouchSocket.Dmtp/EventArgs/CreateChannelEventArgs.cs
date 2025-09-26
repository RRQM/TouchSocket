//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Dmtp;

/// <summary>
/// 创建通道事件类
/// </summary>
public class CreateChannelEventArgs : PluginEventArgs
{
    /// <summary>
    /// 初始化创建通道事件类的实例
    /// </summary>
    /// <param name="channelId">通道的标识符</param>
    /// <param name="metadata">与通道相关的元数据</param>
    public CreateChannelEventArgs(int channelId, Metadata metadata)
    {
        this.ChannelId = channelId;
        this.Metadata = metadata;
    }

    /// <summary>
    /// 通道Id
    /// </summary>
    public int ChannelId { get; private set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Metadata Metadata { get; private set; }
}