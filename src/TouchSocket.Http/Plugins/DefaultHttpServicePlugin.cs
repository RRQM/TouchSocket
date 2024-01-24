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

namespace TouchSocket.Http
{
    /// <summary>
    /// 默认的Http服务。为Http做兜底拦截。该插件应该最后添加。
    /// </summary>
    public sealed class DefaultHttpServicePlugin : PluginBase
    {
        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add<IHttpSocketClient, HttpContextEventArgs>(nameof(IHttpPlugin.OnHttpRequest), this.OnHttpRequest);
            base.Loaded(pluginManager);
        }

        /// <inheritdoc/>
        private Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (e.Context.Response.Responsed)
            {
                return EasyTask.CompletedTask;
            }
            if (e.Context.Request.IsMethod("OPTIONS"))
            {
                var response = e.Context.Response;
                response.SetStatus(204);
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Headers", "*");
                response.Headers.Add("Allow", "OPTIONS, GET, POST");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");

                response.Answer();
            }
            else
            {
                e.Context.Response.UrlNotFind().Answer();
            }

            return EasyTask.CompletedTask;
        }
    }
}