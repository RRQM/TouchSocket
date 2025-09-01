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
/// TouchSocketCoreConfigExtension
/// </summary>
public static class TouchSocketCoreConfigExtension
{
    #region 插件

    /// <summary>
    /// 配置插件。
    /// </summary>
    public static readonly DependencyProperty<Action<IPluginManager>> ConfigurePluginsProperty =
        new("ConfigurePlugins", null);

    /// <summary>
    /// 配置插件。
    /// </summary>
    /// <param name="config">配置对象。</param>
    /// <param name="value">一个作用于IPluginManager的委托，用于配置插件。</param>
    /// <returns>返回更新后的配置对象。</returns>
    public static TouchSocketConfig ConfigurePlugins(this TouchSocketConfig config, Action<IPluginManager> value)
    {
        // 尝试从配置中获取已存在的ConfigurePluginsProperty的值，该值是一个委托
        if (config.TryGetValue(ConfigurePluginsProperty, out var action))
        {
            // 如果已存在委托，则将新的委托添加到现有委托中，以确保多个ConfigurePlugins调用时，所有委托都会被执行
            action += value;
            // 更新配置中的委托
            config.SetValue(ConfigurePluginsProperty, action);
        }
        else
        {
            // 如果配置中不存在委托，则直接添加新的委托
            config.SetValue(ConfigurePluginsProperty, value);
        }
        // 返回更新后的配置对象
        return config;
    }
    #endregion 插件

    #region 容器

    /// <summary>
    /// 配置容器注入。
    /// </summary>
    public static readonly DependencyProperty<Action<IRegistrator>> ConfigureContainerProperty =
        new("ConfigureContainer", null);

    /// <summary>
    /// 容器注册
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IRegistrator> RegistratorProperty =
        new("Registrator", default);

    /// <summary>
    /// 容器提供者
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IResolver> ResolverProperty =
        new("Resolver", null);

    /// <summary>
    /// 配置容器注入。
    /// </summary>
    /// <param name="config">待配置的TouchSocketConfig对象。</param>
    /// <param name="value">一个Action委托，用于注册依赖项。</param>
    /// <returns>返回配置对象，允许链式调用。</returns>
    public static TouchSocketConfig ConfigureContainer(this TouchSocketConfig config, Action<IRegistrator> value)
    {
        // 尝试从配置中获取已存在的Action委托
        if (config.TryGetValue(ConfigureContainerProperty, out var action))
        {
            // 如果已存在委托，则添加新的委托
            action += value;
            // 更新配置中的委托
            config.SetValue(ConfigureContainerProperty, action);
        }
        else
        {
            // 如果不存在委托，则直接设置新的委托
            config.SetValue(ConfigureContainerProperty, value);
        }
        // 返回配置对象，以支持链式调用
        return config;
    }
    #endregion 容器
}