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

using System;

namespace TouchSocket.Http
{
    /// <summary>
    /// HTTP响应结果结构体，实现了IDisposable接口，用于在使用后释放相关资源。
    /// </summary>
    public readonly struct HttpResponseResult : IDisposable
    {
        /// <summary>
        /// 一个操作委托，用于在Dispose时执行特定操作以释放资源。
        /// </summary>
        private readonly Action m_action;

        /// <summary>
        /// 初始化HttpResponseResult结构体。
        /// </summary>
        /// <param name="response">HTTP响应对象，用于处理HTTP请求的响应。</param>
        /// <param name="action">一个Action委托，将在Dispose方法中调用，用于执行资源释放操作。</param>
        public HttpResponseResult(HttpResponse response, Action action)
        {
            this.Response = response;
            this.m_action = action;
        }

        /// <summary>
        /// 获取HTTP响应对象。
        /// </summary>
        public HttpResponse Response { get; }

        /// <summary>
        /// 执行资源释放操作。调用构造函数中传入的Action委托以执行具体释放逻辑。
        /// </summary>
        public void Dispose()
        {
            this.m_action.Invoke();
        }
    }
}