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

using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 完成的请求结果
    /// </summary>
    public class FinishedResult : ResultBase
    {
        /// <summary>
        /// 构造函数：初始化失败请求的结果
        /// </summary>
        /// <param name="resultCode">结果代码，表示失败的类型</param>
        /// <param name="message">错误消息，提供失败的详细信息</param>
        /// <param name="resourceHandle">资源句柄，标识与失败请求相关的资源</param>
        public FinishedResult(ResultCode resultCode, string message, int resourceHandle) : base(resultCode, message)
        {
            this.ResourceHandle = resourceHandle;
        }

        /// <summary>
        /// 完成的请求结果
        /// </summary>
        /// <param name="resultCode">结果代码，表示请求处理的结果</param>
        /// <param name="resourceHandle">资源句柄，用于标识处理的资源</param>
        public FinishedResult(ResultCode resultCode, int resourceHandle) : base(resultCode)
        {
            this.ResourceHandle = resourceHandle;
        }

        /// <summary>
        /// 构造函数：处理失败的请求结果
        /// </summary>
        /// <param name="result">结果对象，包含错误信息</param>
        /// <param name="resourceHandle">资源句柄，用于后续对资源的操作或追踪</param>
        public FinishedResult(Result result, int resourceHandle) : base(result)
        {
            this.ResourceHandle = resourceHandle;
        }

        /// <summary>
        /// 资源句柄
        /// </summary>
        public int ResourceHandle { get; private set; }
    }
}