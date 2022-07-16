//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.Threading;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpc调用上下文
    /// </summary>
    public class JsonRpcServerCallContext : ICallContext
    {
        private object m_caller;
        private JsonRpcContext m_context;
        private MethodInstance m_methodInstance;
        private string m_jsonString;
        private CancellationTokenSource m_tokenSource;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="methodInstance"></param>
        /// <param name="jsonString"></param>
        public JsonRpcServerCallContext(object caller, JsonRpcContext context, MethodInstance methodInstance, string jsonString)
        {
            this.m_caller = caller;
            this.m_context = context;
            this.m_methodInstance = methodInstance;
            this.m_jsonString = jsonString;
        }

        /// <summary>
        /// Json字符串
        /// </summary>
        public string JsonString => this.m_jsonString;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Caller => this.m_caller;

        /// <summary>
        /// JsonRpc上下文
        /// </summary>
        public JsonRpcContext Context => this.m_context;

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