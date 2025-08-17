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

using System;

namespace TouchSocket.Core;

/// <summary>
/// DateHandleAdapterExtension
/// </summary>
public static class DataHandlingAdapterExtension
{
    #region SingleStreamDataHandlingAdapter

    /// <summary>
    /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="config"></param>
    public static void Config(this SingleStreamDataHandlingAdapter adapter, TouchSocketConfig config)
    {
        var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

        if (option.MaxPackageSize.HasValue)
        {
            adapter.MaxPackageSize = option.MaxPackageSize.Value;
        }

        if (option.CacheTimeout.HasValue)
        {
            adapter.CacheTimeout = option.CacheTimeout.Value;
        }

        if (option.CacheTimeoutEnable.HasValue)
        {
            adapter.CacheTimeoutEnable = option.CacheTimeoutEnable.Value;
        }
    }
    //    public static void Config<TRequest>(this DataHandlingAdapterSlim<TRequest> adapter, TouchSocketConfig config)
    //#if NET9_0_OR_GREATER
    //    where TRequest : allows ref struct
    //#endif
    //    {
    //        var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

    //        if (option.MaxPackageSize.HasValue)
    //        {
    //            adapter.MaxPackageSize = option.MaxPackageSize.Value;
    //        }

    //        if (option.CacheTimeout.HasValue)
    //        {
    //            adapter.CacheTimeout = option.CacheTimeout.Value;
    //        }

    //        if (option.CacheTimeoutEnable.HasValue)
    //        {
    //            adapter.CacheTimeoutEnable = option.CacheTimeoutEnable.Value;
    //        }
    //    }

    /// <summary>
    /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="config"></param>
    public static void Config(this DataHandlingAdapter adapter, TouchSocketConfig config)
    {
        var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

        if (option.MaxPackageSize.HasValue)
        {
            adapter.MaxPackageSize = option.MaxPackageSize.Value;
        }
    }

    #endregion SingleStreamDataHandlingAdapter

    /// <summary>
    /// 设置适配器相关的配置
    /// </summary>
    public static readonly DependencyProperty<AdapterOption> AdapterOptionProperty = new("AdapterOption", new AdapterOption());

    /// <summary>
    /// 设置适配器相关的配置
    /// </summary>
    /// <param name="config"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TouchSocketConfig SetAdapterOption(this TouchSocketConfig config, AdapterOption value)
    {
        config.SetValue(AdapterOptionProperty, value);
        return config;
    }
}