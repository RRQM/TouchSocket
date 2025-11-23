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

using System.Collections.Concurrent;

namespace TouchSocket.Core;

/// <summary>
/// 消息通知类。内部使用弱引用保存订阅者，避免强引用导致的内存泄漏。
/// </summary>
public class AppMessenger
{
    private static readonly Lazy<AppMessenger> s_instanceLazy;

    private readonly ConcurrentDictionary<string, ConcurrentList<MessageInstance>> m_tokenAndInstance = new ConcurrentDictionary<string, ConcurrentList<MessageInstance>>();

    static AppMessenger()
    {
        s_instanceLazy = new Lazy<AppMessenger>(() => new AppMessenger());
    }

    /// <summary>
    /// 默认单例实例。
    /// </summary>
    public static AppMessenger Default
    {
        get
        {
            return s_instanceLazy.Value;
        }
    }

    /// <summary>
    /// 是否允许对同一 token 注册多个广播处理器，<see langword="true"/> 表示允许。
    /// </summary>
    public bool AllowMultiple { get; set; }

   
    /// <summary>
    /// 向指定的 <paramref name="token"/> 注册消息处理实例。
    /// </summary>
    /// <param name="token">消息标识符。</param>
    /// <param name="messageInstance">要注册的消息实例。</param>
    public void Add(string token, MessageInstance messageInstance)
    {
        if (this.m_tokenAndInstance.TryGetValue(token, out var value))
        {
            if (!this.AllowMultiple)
            {
                ThrowHelper.ThrowMessageRegisteredException(token);
            }

            value.Add(messageInstance);
        }
        else
        {
            if (!this.m_tokenAndInstance.TryAdd(token, new ConcurrentList<MessageInstance>() { messageInstance }))
            {
                this.Add(token, messageInstance);
            }
        }
    }

   
    /// <summary>
    /// 判断指定的 <paramref name="token"/> 是否可以发送消息（即是否已注册）。
    /// </summary>
    /// <param name="token">消息标识符。</param>
    /// <returns>若已注册返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool CanSendMessage(string token)
    {
        return this.m_tokenAndInstance.ContainsKey(token);
    }

    /// <summary>
    /// 清除所有已注册的消息订阅。
    /// </summary>
    public void Clear()
    {
        this.m_tokenAndInstance.Clear();
    }

    /// <summary>
    /// 获取所有已注册的消息标识符集合。
    /// </summary>
    /// <returns>返回一个包含所有消息标识符的枚举。</returns>
    public IEnumerable<string> GetAllMessage()
    {
        return this.m_tokenAndInstance.Keys;
    }

    /// <summary>
    /// 移除指定 <paramref name="token"/> 的所有订阅。
    /// </summary>
    /// <param name="token">消息标识符。</param>
    public void Remove(string token)
    {
        this.m_tokenAndInstance.TryRemove(token, out _);
    }

    /// <summary>
    /// 按订阅对象移除其在所有 token 下的消息订阅。
    /// </summary>
    /// <param name="messageObject">要移除的订阅对象。</param>
    public void Remove(IMessageObject messageObject)
    {
        var key = new List<string>();

        foreach (var item in this.m_tokenAndInstance)
        {
            foreach (var messageInstance in item.Value)
            {
                if (messageObject == messageInstance.MessageObject)
                {
                    item.Value.Remove(messageInstance);
                    if (item.Value.Count == 0)
                    {
                        key.Add(item.Key);
                    }
                }
            }
        }

        foreach (var item in key)
        {
            this.m_tokenAndInstance.TryRemove(item, out _);
        }
    }

   
    /// <summary>
    /// 异步向指定的 <paramref name="token"/> 广播消息，返回任务以便等待完成。
    /// </summary>
    /// <param name="token">消息标识符。</param>
    /// <param name="parameters">要传递的参数数组。</param>
    /// <returns>表示异步操作的任务。</returns>
    public async Task SendAsync(string token, params object[] parameters)
    {
        if (this.m_tokenAndInstance.TryGetValue(token, out var list))
        {
            var clear = new List<MessageInstance>();

            foreach (var item in list)
            {
                if (!item.Info.IsStatic && !item.WeakReference.TryGetTarget(out _))
                {
                    clear.Add(item);
                    continue;
                }
                await item.InvokeAsync(item.MessageObject, parameters).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            foreach (var item in clear)
            {
                list.Remove(item);
            }
        }
        else
        {
            ThrowHelper.ThrowMessageNotFoundException(token);
        }
    }

    /// <summary>
    /// 异步向指定的 <paramref name="token"/> 发送消息并获取最后一个处理器的返回值。
    /// </summary>
    /// <typeparam name="T">期望的返回值类型。</typeparam>
    /// <param name="token">消息标识符。</param>
    /// <param name="parameters">要传递的参数数组。</param>
    /// <returns>包含最后一个处理器返回值的异步任务；若未找到则返回默认值。</returns>
    public async Task<T> SendAsync<T>(string token, params object[] parameters)
    {
        if (this.m_tokenAndInstance.TryGetValue(token, out var list))
        {
            T result = default;
            var clear = new List<MessageInstance>();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!item.Info.IsStatic && !item.WeakReference.TryGetTarget(out _))
                {
                    clear.Add(item);
                    continue;
                }

                if (i == list.Count - 1)
                {
                    result = await item.InvokeAsync<T>(item.MessageObject, parameters).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    await item.InvokeAsync<T>(item.MessageObject, parameters).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }

            foreach (var item in clear)
            {
                list.Remove(item);
            }
            return result;
        }
        else
        {
            ThrowHelper.ThrowMessageNotFoundException(token);
            return default;
        }
    }
}