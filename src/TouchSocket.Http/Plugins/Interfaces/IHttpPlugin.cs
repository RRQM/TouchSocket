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
//------------------------------------------------------------------------------
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// IHttpPlugin
    /// </summary>
    public interface IHttpPlugin<in TClient> : IPlugin where TClient : IHttpSocketClient
    {

        /// <summary>
        /// 在收到Http请求时。注意：此插件的执行在<see cref="IHttpGetPlugin"/>,<see cref="IHttpPostPlugin"/>,
        /// <see cref="IHttpDeletePlugin"/>,<see cref="IHttpPutPlugin"/>之后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpRequest(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpPlugin
    /// </summary>
    public interface IHttpPlugin : IHttpPlugin<IHttpSocketClient>
    { 
    
    }
}