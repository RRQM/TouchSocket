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

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 定义一个接口，用于处理Web API响应后的操作
    /// </summary>
    public interface IWebApiResponsePlugin : IPlugin
    {
        /// <summary>
        /// 在收到响应之后执行的操作
        /// 此方法允许在Web API客户端收到响应后对其进行自定义处理
        /// </summary>
        /// <param name="client">发出请求的Web API客户端实例</param>
        /// <param name="e">包含响应信息的事件参数</param>
        /// <returns>一个任务对象，代表异步操作</returns>
        Task OnWebApiResponse(IWebApiClientBase client, WebApiEventArgs e);
    }
}