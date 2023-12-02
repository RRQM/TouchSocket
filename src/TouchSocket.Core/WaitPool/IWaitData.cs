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

using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// IWaitData
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWaitData<T> : IDisposable
    {
        /// <summary>
        /// 等待对象的状态
        /// </summary>
        WaitDataStatus Status { get; }

        /// <summary>
        /// 等待结果
        /// </summary>
        T WaitResult { get; }

        /// <summary>
        /// 取消等待
        /// </summary>
        void Cancel();

        /// <summary>
        /// Reset。
        /// 设置<see cref="WaitResult"/>为null。然后重置状态为<see cref="WaitDataStatus.Default"/>
        /// </summary>
        void Reset();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        bool Set();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        bool Set(T waitResult);

        /// <summary>
        /// 加载取消令箭
        /// </summary>
        /// <param name="cancellationToken"></param>
        void SetCancellationToken(CancellationToken cancellationToken);

        /// <summary>
        /// 载入结果
        /// </summary>
        void SetResult(T result);
    }
}