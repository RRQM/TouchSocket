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

#if !NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
    /// </summary>
    public class HttpClientSlim : SetupConfigObject
    {
        private readonly System.Net.Http.HttpClient m_httpClient;

        /// <summary>
        /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpClientSlim(System.Net.Http.HttpClient httpClient = default)
        {
            httpClient ??= new System.Net.Http.HttpClient();
            this.m_httpClient = httpClient;
        }

        /// <summary>
        /// 通讯客户端
        /// </summary>
        public System.Net.Http.HttpClient HttpClient => this.m_httpClient;

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.m_httpClient.BaseAddress ??= config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            base.LoadConfig(config);
        }
    }
}
#endif