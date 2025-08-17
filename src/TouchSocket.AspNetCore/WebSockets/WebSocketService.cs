// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using TouchSocket.Core;
//using TouchSocket.Sockets;

//namespace TouchSocket.Http.WebSockets.AspNetCore;
//public class WebSocketService : ConnectableService<WebSocketSessionClient>
//{
//    #region 字段
//    private readonly InternalClientCollection<WebSocketSessionClient> m_clients = new ();
//    #endregion

//    public override IClientCollection<WebSocketSessionClient> Clients => m_clients;

//    public override int Count => this.m_clients.Count;

//    public override ServerState ServerState =>  ServerState.Running;

//    public override async Task ClearAsync()
//    {
//        foreach (var id in this.GetIds())
//        {
//            if (this.TryGetClient(id, out var client))
//            {
//                await client.CloseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//            }
//        }
//    }

//    public override bool ClientExists(string id)
//    {
//        return this.m_clients.ClientExist(id);
//    }

//    public override IEnumerable<string> GetIds()
//    {
//        return this.m_clients.GetIds();
//    }

//    public override Task ResetIdAsync(string sourceId, string targetId)
//    {
//        throw new NotImplementedException();
//    }

//    public override Task StartAsync()
//    {
//        throw new NotImplementedException();
//    }

//    public override Task<Result> StopAsync(CancellationToken token = default)
//    {
//        throw new NotImplementedException();
//    }

//    protected override WebSocketSessionClient NewClient()
//    {
//        throw new NotImplementedException();
//    }
//}
