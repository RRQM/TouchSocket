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

namespace TouchSocket.Core;

/// <summary>
/// 表示一个等待句柄池的接口。
/// </summary>
/// <typeparam name="TWaitData">等待数据的类型。</typeparam>
/// <typeparam name="TWaitDataAsync">异步等待数据的类型。</typeparam>
/// <typeparam name="T">等待句柄的类型。</typeparam>
public interface IWaitHandlePool<TWaitData, TWaitDataAsync, T> : IDisposableObject
    where T : IWaitHandle
    where TWaitData : IWaitData<T>
    where TWaitDataAsync : IWaitDataAsync<T>
{
    /// <summary>
    /// 获取或设置最大标志值。
    /// </summary>
    int MaxSign { get; set; }

    /// <summary>
    /// 获取或设置最小标志值。
    /// </summary>
    int MinSign { get; set; }

    /// <summary>
    /// 取消池中的所有等待句柄。
    /// </summary>
    void CancelAll();

    /// <summary>
    /// 销毁指定的等待数据。
    /// </summary>
    /// <param name="waitData">要销毁的等待数据。</param>
    [Obsolete("此方法在调用时，可能导致不可控bug，已被弃用，请使用Destroy(int sign)的重载函数直接代替",true)]
    void Destroy(TWaitData waitData);

    /// <summary>
    /// 销毁指定的异步等待数据。
    /// </summary>
    /// <param name="waitData">要销毁的异步等待数据。</param>
    [Obsolete("此方法在调用时，可能导致不可控bug，已被弃用，请使用Destroy(int sign)的重载函数直接代替", true)]
    void Destroy(TWaitDataAsync waitData);

    /// <summary>
    /// 销毁指定的等待数据。
    /// </summary>
    /// <param name="sign">要销毁的等待令箭。</param>
    void Destroy(int sign);

    /// <summary>
    /// 获取一个等待数据对象。
    /// </summary>
    /// <param name="result">要设置在等待数据中的结果。</param>
    /// <param name="autoSign">是否自动标记等待数据。</param>
    /// <returns>等待数据对象。</returns>
    TWaitData GetWaitData(T result, bool autoSign = true);

    /// <summary>
    /// 获取一个等待数据对象并输出其标志。
    /// </summary>
    /// <param name="sign">等待数据的标志。</param>
    /// <returns>等待数据对象。</returns>
    TWaitData GetWaitData(out int sign);

    /// <summary>
    /// 获取一个异步等待数据对象。
    /// </summary>
    /// <param name="result">要设置在等待数据中的结果。</param>
    /// <param name="autoSign">是否自动标记等待数据。</param>
    /// <returns>异步等待数据对象。</returns>
    TWaitDataAsync GetWaitDataAsync(T result, bool autoSign = true);

    /// <summary>
    /// 获取一个异步等待数据对象并输出其标志。
    /// </summary>
    /// <param name="sign">等待数据的标志。</param>
    /// <returns>异步等待数据对象。</returns>
    TWaitDataAsync GetWaitDataAsync(out int sign);

    /// <summary>
    /// 设置指定标志的运行状态。
    /// </summary>
    /// <param name="sign">要设置运行状态的标志。</param>
    /// <returns>如果设置成功则为 true，否则为 false。</returns>
    bool SetRun(int sign);

    /// <summary>
    /// 设置指定标志和等待结果的运行状态。
    /// </summary>
    /// <param name="sign">要设置运行状态的标志。</param>
    /// <param name="waitResult">要设置的等待结果。</param>
    /// <returns>如果设置成功则为 true，否则为 false。</returns>
    bool SetRun(int sign, T waitResult);

    /// <summary>
    /// 设置指定等待结果的运行状态。
    /// </summary>
    /// <param name="waitResult">要设置的等待结果。</param>
    /// <returns>如果设置成功则为 true，否则为 false。</returns>
    bool SetRun(T waitResult);

    /// <summary>
    /// 尝试获取指定标志的等待数据。
    /// </summary>
    /// <param name="sign">要获取等待数据的标志。</param>
    /// <param name="waitData">如果找到则为等待数据。</param>
    /// <returns>如果找到等待数据则为 true，否则为 false。</returns>
    bool TryGetData(int sign, out TWaitData waitData);

    /// <summary>
    /// 尝试获取指定标志的异步等待数据。
    /// </summary>
    /// <param name="sign">要获取异步等待数据的标志。</param>
    /// <param name="waitDataAsync">如果找到则为异步等待数据。</param>
    /// <returns>如果找到异步等待数据则为 true，否则为 false。</returns>
    bool TryGetDataAsync(int sign, out TWaitDataAsync waitDataAsync);
}

/// <summary>
/// 表示一个等待句柄池的接口。
/// </summary>
/// <typeparam name="T">等待句柄的类型。</typeparam>
public interface IWaitHandlePool<T> : IWaitHandlePool<WaitData<T>, WaitDataAsync<T>, T> where T : IWaitHandle
{

}