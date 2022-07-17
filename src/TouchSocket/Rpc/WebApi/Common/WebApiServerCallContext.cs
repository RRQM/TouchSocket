//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.Threading;
using TouchSocket.Http;

namespace TouchSocket.Rpc.WebApi
{
    /// <summary>
    /// WebApi调用上下文
    /// </summary>
    public class WebApiServerCallContext : ICallContext
    {
        private object m_caller;
        private HttpContext m_context;
        private MethodInstance m_methodInstance;
        private CancellationTokenSource m_tokenSource;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="methodInstance"></param>
        public WebApiServerCallContext(object caller, HttpContext context, MethodInstance methodInstance)
        {
            this.m_caller = caller;
            this.m_context = context;
            this.m_methodInstance = methodInstance;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Caller => this.m_caller;

        /// <summary>
        /// Http上下文
        /// </summary>
        public HttpContext Context => this.m_context;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInstance MethodInstance => this.m_methodInstance;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CancellationTokenSource TokenSource
        {
            get
            {
                if (this.m_tokenSource == null)
                {
                    this.m_tokenSource = new CancellationTokenSource();
                }
                return this.m_tokenSource;
            }
        }
    }
}