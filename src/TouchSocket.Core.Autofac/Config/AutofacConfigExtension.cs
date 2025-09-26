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

using Autofac;

namespace TouchSocket.Core;

/// <summary>
/// AutofacConfigExtension
/// </summary>
public static class AutofacConfigExtension
{
    /// <summary>
    /// 配置容器。
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static ContainerBuilder ConfigureContainer(this ContainerBuilder containerBuilder, Action<IRegistrator> action)
    {
        var container = new AutofacContainer(containerBuilder);
        action.Invoke(container);
        return containerBuilder;
    }

    /// <summary>
    /// 使用<see cref="AutofacContainer"/>作为容器。
    /// <para>
    /// 注意：使用此方法，在构建组件时，内部会自行调用<see cref="ContainerBuilder.Build(Autofac.Builder.ContainerBuildOptions)"/>.
    /// </para>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="containerBuilder"></param>
    /// <returns></returns>
    public static TouchSocketConfig UseAutofacContainer(this TouchSocketConfig config, ContainerBuilder containerBuilder)
    {
        config.SetRegistrator(new AutofacContainer(containerBuilder));
        return config;
    }

    /// <summary>
    /// 使用<see cref="AutofacContainer"/>作为容器。
    /// <para>
    /// 注意：使用此方法，在构建组件时，内部会直接使用解决器，不再调用<see cref="ContainerBuilder.Build(Autofac.Builder.ContainerBuildOptions)"/>.
    /// </para>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    public static TouchSocketConfig UseAutofacContainer(this TouchSocketConfig config, Autofac.IContainer container)
    {
        config.SetResolver(new AutofacContainer(container));
        return config;
    }

    /// <summary>
    /// 使用<see cref="AutofacContainer"/>作为容器。
    /// </summary>
    /// <para>
    /// 注意：使用此方法，在构建组件时，内部会自行调用<see cref="ContainerBuilder.Build(Autofac.Builder.ContainerBuildOptions)"/>.
    /// </para>
    /// <param name="config"></param>
    /// <returns></returns>
    public static TouchSocketConfig UseAutofacContainer(this TouchSocketConfig config)
    {
        return UseAutofacContainer(config, new ContainerBuilder());
    }
}