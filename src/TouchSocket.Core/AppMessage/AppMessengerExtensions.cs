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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// AppMessengerExtensions
    /// </summary>
    public static class AppMessengerExtensions
    {
        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        public static void Register(this AppMessenger appMessenger, IMessageObject messageObject)
        {
            var methods = GetInstanceMethods(messageObject.GetType());
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is AppMessageAttribute att)
                    {
                        if (string.IsNullOrEmpty(att.Token))
                        {
                            Register(appMessenger, messageObject, method.Name, method);
                        }
                        else
                        {
                            Register(appMessenger, messageObject, att.Token, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        /// <param name="token"></param>
        /// <param name="methodInfo"></param>
        /// <exception cref="MessageRegisteredException"></exception>
        public static void Register(this AppMessenger appMessenger, IMessageObject messageObject, string token, MethodInfo methodInfo)
        {
            appMessenger.Add(token, new MessageInstance(methodInfo, messageObject));
        }

        ///// <summary>
        ///// 注册消息
        ///// </summary>
        ///// <typeparam name="T1"></typeparam>
        ///// <typeparam name="T2"></typeparam>
        ///// <typeparam name="TReturn"></typeparam>
        ///// <param name="appMessenger"></param>
        ///// <param name="func"></param>
        ///// <param name="token"></param>
        //public static void Register(this AppMessenger appMessenger, Delegate func, string token = default)
        //{
        //    RegisterDelegate(appMessenger, token, func);
        //}

        /// <summary>
        /// 注册类的静态消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterStatic<T>(this AppMessenger appMessenger) where T : IMessageObject
        {
            RegisterStatic(appMessenger, typeof(T));
        }

        /// <summary>
        /// 注册类的静态消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="type"></param>
        /// <exception cref="NotSupportedException"></exception>
        public static void RegisterStatic(this AppMessenger appMessenger, Type type)
        {
            var methods = GetStaticMethods(type);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is AppMessageAttribute att)
                    {
                        if (string.IsNullOrEmpty(att.Token))
                        {
                            Register(appMessenger, null, method.Name, method);
                        }
                        else
                        {
                            Register(appMessenger, null, att.Token, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 卸载消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        public static void Unregister(this AppMessenger appMessenger, IMessageObject messageObject)
        {
            appMessenger.Remove(messageObject);
        }

        /// <summary>
        /// 移除注册
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Unregister(this AppMessenger appMessenger, string token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            appMessenger.Remove(token);
        }

        private static MethodInfo[] GetInstanceMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static MethodInfo[] GetStaticMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        //private static void RegisterDelegate(this AppMessenger appMessenger, string token, Delegate dele)
        //{
        //    if (!typeof(Task).IsAssignableFrom(dele.Method.ReturnType))
        //    {
        //        throw new Exception("注册委托的返回值必须继承自Task或其泛型");
        //    }
        //    if (token.HasValue())
        //    {
        //        appMessenger.Add(token, new MessageInstance(dele));
        //        return;
        //    }
        //    var attributes = dele.Method.GetCustomAttributes();
        //    foreach (var attribute in attributes)
        //    {
        //        if (attribute is AppMessageAttribute att)
        //        {
        //            if (token.IsNullOrEmpty())
        //            {
        //                token = string.IsNullOrEmpty(att.Token) ? dele.Method.Name : att.Token;
        //            }

        //            appMessenger.Add(token, new MessageInstance(dele.Method, dele.Target));
        //        }
        //    }
        //}
    }
}