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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace WebApiServerApp;

/// <summary>
/// 鉴权插件
/// </summary>
internal class AuthenticationPlugin : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        //string aut = e.Context.Request.Headers["Authorization"];
        //if (aut.IsNullOrEmpty())//授权header为空
        //{
        //   await e.Context.Response
        //        .SetStatus(401, "授权失败")
        //        .AnswerAsync();
        //    return;
        //}

        //伪代码，假设使用jwt解码成功。那就执行下一个插件。
        //if (jwt.Encode(aut))
        //{
        //   此处可以做一些授权相关的。
        //}
        await e.InvokeNext();
    }
}
