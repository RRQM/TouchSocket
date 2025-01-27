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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 消息通知类。内部全为弱引用。
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
    /// 默认单例实例
    /// </summary>
    public static AppMessenger Default
    {
        get
        {
            return s_instanceLazy.Value;
        }
    }

    /// <summary>
    /// 允许多广播注册
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="token"></param>
    /// <param name="messageInstance"></param>
    /// <exception cref="MessageRegisteredException"></exception>
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
    /// 判断能否触发该消息，意味着该消息是否已经注册。
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool CanSendMessage(string token)
    {
        return this.m_tokenAndInstance.ContainsKey(token);
    }

    /// <summary>
    /// 清除所有消息
    /// </summary>
    public void Clear()
    {
        this.m_tokenAndInstance.Clear();
    }

    /// <summary>
    /// 获取所有消息
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllMessage()
    {
        return this.m_tokenAndInstance.Keys;
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="token"></param>
    public void Remove(string token)
    {
        this.m_tokenAndInstance.TryRemove(token, out _);
    }

    /// <summary>
    /// 按对象移除
    /// </summary>
    /// <param name="messageObject"></param>
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
    /// 发送消息
    /// </summary>
    /// <param name="token"></param>
    /// <param name="parameters"></param>
    /// <exception cref="MessageNotFoundException"></exception>
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
    /// 发送消息，当多播时，只返回最后一个返回值
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="token"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="MessageNotFoundException"></exception>
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