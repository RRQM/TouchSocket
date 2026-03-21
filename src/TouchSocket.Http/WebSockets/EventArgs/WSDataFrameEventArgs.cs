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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WS数据事件类
/// </summary>
public class WSDataFrameEventArgs : PluginEventArgs
{
    /// <summary>
    /// 初始化 <see cref="WSDataFrameEventArgs"/> 类的新实例。
    /// </summary>
    /// <param name="dataFrame">WS数据帧。</param>
    public WSDataFrameEventArgs(WSDataFrame dataFrame)
    {
        this.DataFrame = dataFrame;
    }

    /// <summary>
    /// 初始化 <see cref="WSDataFrameEventArgs"/> 类的新实例。
    /// </summary>
    public WSDataFrameEventArgs()
    {
    }

    /// <summary>
    /// WS数据帧。
    /// </summary>
    public WSDataFrame DataFrame { get; private set; }

    /// <summary>
    /// 重置事件参数并设置 WS 数据帧。
    /// </summary>
    /// <param name="dataFrame">WS数据帧。</param>
    public WSDataFrameEventArgs Reset(WSDataFrame dataFrame)
    {
        this.DataFrame = dataFrame;
        base.Reset();
        return this;
    }
}