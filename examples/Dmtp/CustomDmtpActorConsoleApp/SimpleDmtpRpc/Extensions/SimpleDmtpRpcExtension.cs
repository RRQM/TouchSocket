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

using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc;

internal static class SimpleDmtpRpcExtension
{
    #region DependencyProperty

    /// <summary>
    /// SimpleDmtpRpcActor
    /// </summary>
    public static readonly DependencyProperty<ISimpleDmtpRpcActor> SimpleDmtpRpcActorProperty =
        new("SimpleDmtpRpcActor", default);

    #endregion DependencyProperty

    #region 插件扩展

    /// <summary>
    /// 使用SimpleDmtpRpc插件
    /// </summary>
    /// <param name="pluginManager"></param>
    /// <returns></returns>
    public static SimpleDmtpRpcFeature UseSimpleDmtpRpc(this IPluginManager pluginManager)
    {
        return pluginManager.Add<SimpleDmtpRpcFeature>();
    }

    #endregion 插件扩展

    /// <summary>
    /// 从<see cref="DmtpActor"/>中获取<see cref="ISimpleDmtpRpcActor"/>
    /// </summary>
    /// <param name="smtpActor"></param>
    /// <returns></returns>
    public static ISimpleDmtpRpcActor GetSimpleDmtpRpcActor(this IDmtpActor smtpActor)
    {
        return smtpActor.GetValue(SimpleDmtpRpcActorProperty);
    }

    /// <summary>
    /// 从<see cref="IDmtpActorObject"/>中获取<see cref="ISimpleDmtpRpcActor"/>，以实现Rpc调用功能。
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ISimpleDmtpRpcActor GetSimpleDmtpRpcActor(this IDmtpActorObject client)
    {
        var smtpRpcActor = client.DmtpActor.GetSimpleDmtpRpcActor();
        if (smtpRpcActor is null)
        {
            throw new ArgumentNullException(nameof(smtpRpcActor), "SimpleRpcAcotr为空，请检查是否已启用UseSimpleDmtpRpc");
        }
        return smtpRpcActor;
    }

    /// <summary>
    /// 向<see cref="DmtpActor"/>中设置<see cref="ISimpleDmtpRpcActor"/>
    /// </summary>
    /// <param name="smtpActor"></param>
    /// <param name="smtpRpcActor"></param>
    internal static void SetSimpleDmtpRpcActor(this IDmtpActor smtpActor, ISimpleDmtpRpcActor smtpRpcActor)
    {
        smtpActor.SetValue(SimpleDmtpRpcActorProperty, smtpRpcActor);
    }
}