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
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 异步等待数据接口。
/// </summary>
/// <typeparam name="T">等待结果的类型。</typeparam>
public interface IWaitDataAsync<T> : IWaitDataBase<T>
{
    /// <summary>
    /// 异步等待指定的时间间隔。
    /// </summary>
    /// <param name="timeSpan">等待的时间间隔。</param>
    /// <returns>等待数据的状态。</returns>
    Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan);

    /// <summary>
    /// 异步等待指定的毫秒数。
    /// </summary>
    /// <param name="millisecond">等待的毫秒数。</param>
    /// <returns>等待数据的状态。</returns>
    Task<WaitDataStatus> WaitAsync(int millisecond);
}