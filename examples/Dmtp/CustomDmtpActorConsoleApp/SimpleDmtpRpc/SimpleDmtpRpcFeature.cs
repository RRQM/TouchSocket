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

using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc;

internal class SimpleDmtpRpcFeature : PluginBase, IDmtpConnectingPlugin, IDmtpReceivedPlugin
{
    private readonly Dictionary<string, MethodModel> m_pairs = new Dictionary<string, MethodModel>();

    public async Task OnDmtpConnecting(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var actor = new SimpleDmtpRpcActor(client.DmtpActor)
        {
            TryFindMethod = this.TryFindMethod
        };
        client.DmtpActor.AddActor(actor);
        await e.InvokeNext();
    }

    private MethodModel TryFindMethod(string methodName)
    {
        if (this.m_pairs.TryGetValue(methodName, out var methodModel))
        {
            return methodModel;
        }
        return default;
    }

    public void RegisterRpc(object server)
    {
        if (server is null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        foreach (var item in server.GetType().GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public))
        {
            this.m_pairs.Add(item.Name, new MethodModel(item, server));
        }
    }

    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        if (client.DmtpActor.GetSimpleDmtpRpcActor() is SimpleDmtpRpcActor actor)
        {
            if (await actor.InputReceivedData(e.DmtpMessage))
            {
                e.Handled = true;
                return;
            }
        }
        await e.InvokeNext();
    }
}