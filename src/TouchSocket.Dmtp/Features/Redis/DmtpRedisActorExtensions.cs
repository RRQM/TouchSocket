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

using TouchSocket.Resources;

namespace TouchSocket.Dmtp.Redis;

/// <summary>
/// 定义一个静态类，用于扩展DmtpRedisActor的功能
/// </summary>
public static class DmtpRedisActorExtensions
{
    ///// <summary>
    ///// 获取或设置RedisActor的注入键。
    ///// </summary>
    //public static readonly DependencyProperty<IDmtpRedisActor> DmtpRedisActorProperty =
    //    new("DmtpRedisActor", null);

    /// <summary>
    /// 获取<see cref="IDmtpRedisActor"/>
    /// </summary>
    /// <param name="client">要获取<see cref="IDmtpRedisActor"/>的<see cref="IDmtpActorObject"/>实例</param>
    /// <returns><see cref="IDmtpRedisActor"/>实例</returns>
    /// <exception cref="Exception">当<see cref="IDmtpRedisActor"/>为<see langword="null"/>时抛出<see cref="ArgumentException"/></exception>
    public static IDmtpRedisActor GetDmtpRedisActor(this IDmtpActorObject client)
    {
        var actor = client.DmtpActor.GetDmtpRedisActor();
        ThrowHelper.ThrowArgumentNullExceptionIf(actor, nameof(actor), TouchSocketDmtpResource.RedisActorNull);
        return actor;
    }

    /// <summary>
    /// 从<see cref="DmtpActor"/>中获得<see cref="IDmtpRedisActor"/>
    /// </summary>
    /// <param name="dmtpActor">要从中获取<see cref="IDmtpRedisActor"/>的<see cref="DmtpActor"/>实例</param>
    /// <returns>返回从<see cref="DmtpActor"/>中获取的<see cref="IDmtpRedisActor"/>实例</returns>
    public static IDmtpRedisActor GetDmtpRedisActor(this IDmtpActor dmtpActor)
    {
        return dmtpActor.GetActor<DmtpRedisActor>();
    }

    /// <summary>
    /// 使用Redis插件。仅：Dmtp端会生效。
    /// </summary>
    /// <param name="pluginManager">插件管理器，用于管理插件。</param>
    /// <returns>返回Redis功能插件。</returns>
    public static RedisFeature UseDmtpRedis(this IPluginManager pluginManager)
    {
        // 添加RedisFeature到插件管理器，使Dmtp端能够使用Redis插件。
        return pluginManager.Add<RedisFeature>();
    }
}