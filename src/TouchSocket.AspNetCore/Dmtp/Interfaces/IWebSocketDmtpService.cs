//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.AspNetCore
{
    /// <summary>
    /// IWebSocketDmtpService服务器接口
    /// </summary>
    public interface IWebSocketDmtpService : IDmtpService, ISetupConfigObject
    {
        /// <summary>
        /// 转换客户端
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task SwitchClientAsync(HttpContext context);

        /// <summary>
        /// 重置Id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        void ResetId(string oldId, string newId);

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        IEnumerable<WebSocketDmtpSocketClient> GetClients();

        /// <summary>
        /// 获取所有在线客户端的Id集合。
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIds();
    }
}