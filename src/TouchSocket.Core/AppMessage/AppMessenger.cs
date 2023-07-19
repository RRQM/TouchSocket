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
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 消息通知类。内部全为弱引用。
    /// </summary>
    public class AppMessenger
    {
        private static AppMessenger m_instance;
        private readonly ReaderWriterLockSlim m_lockSlim = new ReaderWriterLockSlim();
        private readonly Dictionary<string, List<MessageInstance>> m_tokenAndInstance = new Dictionary<string, List<MessageInstance>>();

        /// <summary>
        /// 默认单例实例
        /// </summary>
        public static AppMessenger Default
        {
            get
            {
                if (m_instance != null)
                {
                    return m_instance;
                }
                lock (typeof(AppMessenger))
                {
                    if (m_instance != null)
                    {
                        return m_instance;
                    }
                    m_instance = new AppMessenger();
                    return m_instance;
                }
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
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                if (this.m_tokenAndInstance.ContainsKey(token))
                {
                    if (!this.AllowMultiple)
                    {
                        throw new MessageRegisteredException(TouchSocketCoreResource.TokenExisted.GetDescription(token));
                    }
                    this.m_tokenAndInstance[token].Add(messageInstance);
                }
                else
                {
                    this.m_tokenAndInstance.Add(token, new List<MessageInstance>()
                    {
                     messageInstance
                    });
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
            using (var readLock = new ReadLock(this.m_lockSlim))
            {
                return this.m_tokenAndInstance.ContainsKey(token);
            }
        }

        /// <summary>
        /// 清除所有消息
        /// </summary>
        public void Clear()
        {
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                this.m_tokenAndInstance.Clear();
            }
        }

        /// <summary>
        /// 获取所有消息
        /// </summary>
        /// <returns></returns>
        public string[] GetAllMessage()
        {
            using (var readLock = new ReadLock(this.m_lockSlim))
            {
                return this.m_tokenAndInstance.Keys.ToArray();
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="token"></param>
        public void Remove(string token)
        {
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                this.m_tokenAndInstance.Remove(token);
            }
        }

        /// <summary>
        /// 按对象移除
        /// </summary>
        /// <param name="messageObject"></param>
        public void Remove(IMessageObject messageObject)
        {
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                var key = new List<string>();

                foreach (var item in this.m_tokenAndInstance.Keys)
                {
                    foreach (var item2 in this.m_tokenAndInstance[item].ToArray())
                    {
                        if (messageObject == item2.MessageObject)
                        {
                            this.m_tokenAndInstance[item].Remove(item2);
                            if (this.m_tokenAndInstance[item].Count == 0)
                            {
                                key.Add(item);
                            }
                        }
                    }
                }

                foreach (var item in key)
                {
                    this.m_tokenAndInstance.Remove(item);
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <exception cref="MessageNotFoundException"></exception>
        public Task SendAsync(string token, params object[] parameters)
        {
            return Task.Run(() =>
             {
                 using (var readLock = new ReadLock(this.m_lockSlim))
                 {
                     if (this.m_tokenAndInstance.TryGetValue(token, out var list))
                     {
                         var clear = new List<MessageInstance>();

                         foreach (var item in list)
                         {
                             if (!item.Static && !item.WeakReference.TryGetTarget(out _))
                             {
                                 clear.Add(item);
                                 continue;
                             }
                             try
                             {
                                 item.Invoke(item.MessageObject, parameters);
                             }
                             catch
                             {
                             }
                         }

                         foreach (var item in clear)
                         {
                             list.Remove(item);
                         }
                     }
                     else
                     {
                         throw new MessageNotFoundException(TouchSocketCoreResource.MessageNotFound.GetDescription(token));
                     }
                 }
             });
        }

        /// <summary>
        /// 发送消息，当多播时，只返回最后一个返回值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="MessageNotFoundException"></exception>
        public Task<T> SendAsync<T>(string token, params object[] parameters)
        {
            return Task.Run(() =>
             {
                 using (var readLock = new ReadLock(this.m_lockSlim))
                 {
                     if (this.m_tokenAndInstance.TryGetValue(token, out var list))
                     {
                         T result = default;
                         var clear = new List<MessageInstance>();
                         for (var i = 0; i < list.Count; i++)
                         {
                             var item = list[i];
                             if (!item.Static && !item.WeakReference.TryGetTarget(out _))
                             {
                                 clear.Add(item);
                                 continue;
                             }

                             try
                             {
                                 if (i == list.Count - 1)
                                 {
                                     result = (T)item.Invoke(item.MessageObject, parameters);
                                 }
                                 else
                                 {
                                     item.Invoke(item.MessageObject, parameters);
                                 }
                             }
                             catch
                             {
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
                         throw new MessageNotFoundException(TouchSocketCoreResource.MessageNotFound.GetDescription(token));
                     }
                 }
             });
        }
    }
}