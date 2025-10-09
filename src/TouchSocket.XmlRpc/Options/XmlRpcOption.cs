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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.XmlRpc;

public class XmlRpcOption
{
    public XmlRpcOption()
    {
        this.SetAllowXmlRpc();
    }
    public Func<IHttpSessionClient, HttpContext, Task<bool>> AllowXmlRpc { get; set; }

    /// <summary>
    /// 设置允许XmlRpc的委托。
    /// </summary>
    /// <param name="allowXmlRpc">允许XmlRpc的委托</param>
    public void SetAllowXmlRpc(Func<IHttpSessionClient, HttpContext, Task<bool>> allowXmlRpc)
    {
        this.AllowXmlRpc = allowXmlRpc;
    }

    /// <summary>
    /// 设置允许XmlRpc的Url。
    /// </summary>
    /// <param name="url">允许的Url，默认为"/XmlRpc"。</param>
    public void SetAllowXmlRpc(string url = "/XmlRpc")
    {
        this.AllowXmlRpc = (client, context) => Task.FromResult(context.Request.UrlEquals(url));
    }

    /// <summary>
    /// 设置允许XmlRpc的委托。
    /// </summary>
    /// <param name="allowXmlRpc">允许XmlRpc的委托</param>
    public void SetAllowXmlRpc(Func<IHttpSessionClient, HttpContext, bool> allowXmlRpc)
    {
        this.AllowXmlRpc = (client, context) => Task.FromResult(allowXmlRpc(client, context));
    }
}
