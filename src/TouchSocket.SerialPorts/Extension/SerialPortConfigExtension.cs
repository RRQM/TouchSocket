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

namespace TouchSocket.SerialPorts;

/// <summary>
/// SerialPortConfigExtension
/// </summary>
public static class SerialPortConfigExtension
{
    /// <summary>
    /// 设置串口适配器
    /// </summary>
    [Obsolete("请使用TouchSocketConfigExtension.DataHandlingAdapterProperty或SetDataHandlingAdapter代替。", false)]
    public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> SerialDataHandlingAdapterProperty =
        new("SerialDataHandlingAdapter", null);

    /// <summary>
    /// 串口属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<SerialPortOption> SerialPortOptionProperty =
        new("SerialPortOption", default);

    #region 过时

    /// <inheritdoc cref="TouchSocketConfigExtension.SingleStreamDataHandlingAdapterProperty"/>
    [Obsolete("请使用SetSingleStreamDataHandlingAdapter代替。", false)]
    public static TDependencyObject SetSerialDataHandlingAdapter<TDependencyObject>(this TDependencyObject dependencyObject, Func<SingleStreamDataHandlingAdapter> value)
        where TDependencyObject : TouchSocketConfig
    {
        dependencyObject.SetValue(TouchSocketConfigExtension.SingleStreamDataHandlingAdapterProperty, value);
        return dependencyObject;
    }

    /// <inheritdoc cref="TouchSocketConfigExtension.SingleStreamDataHandlingAdapterProperty"/>
    [Obsolete("请使用GetDataHandlingAdapter代替。", false)]
    public static Func<SingleStreamDataHandlingAdapter> GetSerialDataHandlingAdapter<TDependencyObject>(this TDependencyObject dependencyObject)
        where TDependencyObject : TouchSocketConfig
    {
        return dependencyObject.GetValue(TouchSocketConfigExtension.SingleStreamDataHandlingAdapterProperty);
    }

    #endregion 过时
}