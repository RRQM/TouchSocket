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
    /// 定义一个用于处理Web API请求的插件接口
    /// </summary>
    public interface IWebApiRequestPlugin : IPlugin
    {
        /// <summary>
        /// 在请求之前
        /// </summary>
        /// <param name="client">Web API客户端基类实例，用于发送请求</param>
        /// <param name="e">包含请求相关信息的事件参数</param>
        /// <returns>一个任务对象，用于异步操作</returns>
        Task OnWebApiRequest(IWebApiClientBase client, WebApiEventArgs e);
    }
}