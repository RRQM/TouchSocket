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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 调用上下文
    /// </summary>
    public interface ICallContext
    {
        /// <summary>
        /// 函数实例
        /// </summary>
        MethodInstance MethodInstance { get; }

        /// <summary>
        /// 实际调用者
        /// </summary>
        object Caller { get; }

        /// <summary>
        /// 可取消的调用令箭
        /// </summary>
        CancellationTokenSource TokenSource { get; }
    }
}