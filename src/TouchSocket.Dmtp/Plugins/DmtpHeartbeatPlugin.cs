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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 基于Dmtp的心跳插件。服务器和客户端均适用
/// </summary>
[PluginOption(Singleton = true)]
public class DmtpHeartbeatPlugin : HeartbeatPlugin, IDmtpHandshakedPlugin
{
    /// <inheritdoc/>
    public async Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        _ = Task.Factory.StartNew(async () =>
        {
            var failedCount = 0;
            while (true)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                await Task.Delay(this.Tick);
                if (client.DmtpActor == null || !client.DmtpActor.Online)
                {
                    return;
                }
                if (DateTime.UtcNow - client.DmtpActor.LastActiveTime < this.Tick)
                {
                    continue;
                }

                if (await client.DmtpActor.PingAsync())
                {
                    failedCount = 0;
                }
                else
                {
                    failedCount++;
                    if (failedCount > this.MaxFailCount)
                    {
                        await client.DmtpActor.CloseAsync("自动心跳失败次数达到最大，已断开连接。");
                    }
                }
            }
        }, TaskCreationOptions.LongRunning);

        await e.InvokeNext();
    }
}