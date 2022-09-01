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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TouchSocket.Resources;

namespace TouchSocket.Core.Run
{
    /// <summary>
    /// 消息通知类
    /// </summary>
    public class AppMessenger<TMessage> where TMessage : IMessage
    {
        private bool allowMultiple = false;

        private readonly ConcurrentDictionary<string, List<TokenInstance>> tokenAndInstance = new ConcurrentDictionary<string, List<TokenInstance>>();

        /// <summary>
        /// 允许多广播注册
        /// </summary>
        public bool AllowMultiple
        {
            get => this.allowMultiple;
            set => this.allowMultiple = value;
        }

        /// <summary>
        /// 判断能否触发该消息，意味着该消息是否已经注册。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CanSendMessage(string token)
        {
            return this.tokenAndInstance.ContainsKey(token);
        }

        /// <summary>
        /// 清除所有消息
        /// </summary>
        public void Clear()
        {
            this.tokenAndInstance.Clear();
        }

        /// <summary>
        /// 获取所有消息
        /// </summary>
        /// <returns></returns>
        public string[] GetAllMessage()
        {
            return this.tokenAndInstance.Keys.ToArray();
        }

        /// <summary>
        /// 注册已加载程序集中直接或间接继承自IMassage接口的所有类，并创建新实例
        /// </summary>
        public void RegistAll()
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] t1 = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(TMessage))).ToArray();
                    types.AddRange(t1);
                }
                catch
                {
                }
            }
            foreach (var v in types)
            {
                TMessage message = (TMessage)Activator.CreateInstance(v);
                this.Register(message);
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageObject"></param>
        /// <param name="action"></param>
        public void Register(TMessage messageObject, Action action)
        {
            this.Register(messageObject, action.Method.Name, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>() where T : TMessage
        {
            this.Register((T)Activator.CreateInstance(typeof(T)));
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <exception cref="MessageRegisteredException"></exception>
        public void Register(TMessage messageObject, string token, Action action)
        {
            if (this.allowMultiple || !this.tokenAndInstance.ContainsKey(token))
            {
                TokenInstance tokenInstance = new TokenInstance();
                tokenInstance.MessageObject = messageObject;
                tokenInstance.Method = new Reflection.Method(action.Method);
                var list = this.tokenAndInstance.GetOrAdd(token, (s) => { return new List<TokenInstance>(); });
                list.Add(tokenInstance);
            }
            else
            {
                throw new MessageRegisteredException(TouchSocketRes.TokenExisted.GetDescription(token));
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageObject"></param>
        public void Register(TMessage messageObject)
        {
            MethodInfo[] methods = messageObject.GetType().GetMethods();
            foreach (var method in methods)
            {
                IEnumerable<Attribute> attributes = method.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is AppMessageAttribute att)
                    {
                        if (string.IsNullOrEmpty(att.Token))
                        {
                            this.Register(messageObject, method.Name, method);
                        }
                        else
                        {
                            this.Register(messageObject, att.Token, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="methodInfo"></param>
        /// <exception cref="MessageRegisteredException"></exception>
        public void Register(TMessage messageObject, string token, MethodInfo methodInfo)
        {
            if (this.allowMultiple || !this.tokenAndInstance.ContainsKey(token))
            {
                TokenInstance tokenInstance = new TokenInstance();
                tokenInstance.MessageObject = messageObject;
                tokenInstance.Method = new Reflection.Method(methodInfo);
                var list = this.tokenAndInstance.GetOrAdd(token, (s) => { return new List<TokenInstance>(); });
                list.Add(tokenInstance);
            }
            else
            {
                throw new MessageRegisteredException(TouchSocketRes.TokenExisted.GetDescription(token));
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject"></param>
        /// <param name="action"></param>
        public void Register<T>(TMessage messageObject, Action<T> action)
        {
            this.Register(messageObject, action.Method.Name, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <exception cref="MessageRegisteredException"></exception>
        public void Register<T>(TMessage messageObject, string token, Action<T> action)
        {
            if (this.allowMultiple || !this.tokenAndInstance.ContainsKey(token))
            {
                TokenInstance tokenInstance = new TokenInstance();
                tokenInstance.MessageObject = messageObject;
                tokenInstance.Method = new Reflection.Method(action.Method);
                var list = this.tokenAndInstance.GetOrAdd(token, (s) => { return new List<TokenInstance>(); });
                list.Add(tokenInstance);
            }
            else
            {
                throw new MessageRegisteredException(TouchSocketRes.TokenExisted.GetDescription(token));
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <typeparam name="TReturn">返回值类型</typeparam>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        public void Register<T, TReturn>(TMessage messageObject, string token, Func<T, TReturn> action)
        {
            if (this.allowMultiple || !this.tokenAndInstance.ContainsKey(token))
            {
                TokenInstance tokenInstance = new TokenInstance();
                tokenInstance.MessageObject = messageObject;
                tokenInstance.Method = new Reflection.Method(action.Method);
                var list = this.tokenAndInstance.GetOrAdd(token, (s) => { return new List<TokenInstance>(); });
                list.Add(tokenInstance);
            }
            else
            {
                throw new MessageRegisteredException(TouchSocketRes.TokenExisted.GetDescription(token));
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="TReturn">返回值类型</typeparam>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        public void Register<TReturn>(TMessage messageObject, string token, Func<TReturn> action)
        {
            if (this.allowMultiple || !this.tokenAndInstance.ContainsKey(token))
            {
                TokenInstance tokenInstance = new TokenInstance();
                tokenInstance.MessageObject = messageObject;
                tokenInstance.Method = new Reflection.Method(action.Method);
                var list = this.tokenAndInstance.GetOrAdd(token, (s) => { return new List<TokenInstance>(); });
                list.Add(tokenInstance);
            }
            else
            {
                throw new MessageRegisteredException(TouchSocketRes.TokenExisted.GetDescription(token));
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <exception cref="MessageNotFoundException"></exception>
        public void Send(string token, params object[] parameters)
        {
            if (this.tokenAndInstance.TryGetValue(token, out List<TokenInstance> list))
            {
                foreach (var item in list)
                {
                    item.Method.Invoke(item.MessageObject, parameters);
                }
            }
            else
            {
                throw new MessageNotFoundException(TouchSocketRes.MessageNotFound.GetDescription(token));
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
        public T Send<T>(string token, params object[] parameters)
        {
            if (this.tokenAndInstance.TryGetValue(token, out List<TokenInstance> list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (i == list.Count - 1)
                    {
                        return (T)item.Method.Invoke(item.MessageObject, parameters);
                    }
                    else
                    {
                        item.Method.Invoke(item.MessageObject, parameters);
                    }
                }
                return default;
            }
            else
            {
                throw new MessageNotFoundException(TouchSocketRes.MessageNotFound.GetDescription(token));
            }
        }

        /// <summary>
        /// 卸载消息
        /// </summary>
        /// <param name="messageObject"></param>
        public void Unregister(TMessage messageObject)
        {
            List<string> key = new List<string>();

            foreach (var item in this.tokenAndInstance.Keys)
            {
                foreach (var item2 in this.tokenAndInstance[item].ToArray())
                {
                    if ((IMessage)messageObject == item2.MessageObject)
                    {
                        this.tokenAndInstance[item].Remove(item2);
                        if (this.tokenAndInstance[item].Count == 0)
                        {
                            key.Add(item);
                        }
                    }
                }
            }

            foreach (var item in key)
            {
                this.tokenAndInstance.TryRemove(item, out _);
            }
        }

        /// <summary>
        /// 卸载消息
        /// </summary>
        public void Unregister(string token)
        {
            this.tokenAndInstance.TryRemove(token, out _);
        }
    }

    /// <summary>
    /// 消息通知类
    /// </summary>
    public class AppMessenger : AppMessenger<IMessage>
    {
        private static AppMessenger instance;

        /// <summary>
        /// 默认单例实例
        /// </summary>
        public static AppMessenger Default
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppMessenger();
                }

                return instance;
            }
        }
    }
}