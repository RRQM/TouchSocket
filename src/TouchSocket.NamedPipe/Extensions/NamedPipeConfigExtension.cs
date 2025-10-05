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

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道配置扩展。
/// </summary>
public static class NamedPipeConfigExtension
{
    /// <summary>
    /// 命名管道数据处理适配器属性。
    /// </summary>
    [GeneratorProperty(TargetType =typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> NamedPipeDataHandlingAdapterProperty = new("NamedPipeDataHandlingAdapter", null
        );

    /// <summary>
    /// 直接单个配置命名管道监听的地址组。所需类型<see cref="Action"/>。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig),ActionMode =true)]
    public static readonly DependencyProperty<List<NamedPipeListenOption>> NamedPipeListenOptionProperty = new("NamedPipeListenOption", null);

    /// <summary>
    /// 命名管道名称。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<string> PipeNameProperty = new("PipeName", null);

    /// <summary>
    /// 命名管道的服务主机名称。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<string> PipeServerNameProperty = new("PipeServerName", ".");
}