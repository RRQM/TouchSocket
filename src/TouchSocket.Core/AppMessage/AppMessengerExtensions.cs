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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 针对 <see cref="AppMessenger"/> 的扩展方法，提供基于特性和反射的注册/注销辅助方法。
/// </summary>
public static class AppMessengerExtensions
{
    /// <summary>
    /// 将实现 <see cref="IMessageObject"/> 的实例中标记有 <see cref="AppMessageAttribute"/> 的方法注册到 <see cref="AppMessenger"/>。
    /// </summary>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    /// <param name="messageObject">实现了 <see cref="IMessageObject"/> 的订阅对象。</param>
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
    /// 将指定的 <paramref name="methodInfo"/> 以给定的 <paramref name="token"/> 注册到 <see cref="AppMessenger"/>。
    /// </summary>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    /// <param name="messageObject">承载方法的对象实例；静态方法时可为 <see langword="null"/>。</param>
    /// <param name="token">消息标识符，可用于后续的发送或解绑。</param>
    /// <param name="methodInfo">要注册的方法的 <see cref="MethodInfo"/>。</param>
    /// <exception cref="MessageRegisteredException">当同一 token 已被注册且未允许多注册时抛出。</exception>
    public static void Register(this AppMessenger appMessenger, IMessageObject messageObject, string token, MethodInfo methodInfo)
    {
        appMessenger.Add(token, new MessageInstance(methodInfo, messageObject));
    }


    /// <summary>
    /// 注册类型 <typeparamref name="T"/> 中标记为 <see cref="AppMessageAttribute"/> 的静态方法到 <see cref="AppMessenger"/>。
    /// </summary>
    /// <typeparam name="T">包含静态消息方法的类型，必须实现 <see cref="IMessageObject"/>。</typeparam>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    public static void RegisterStatic<T>(this AppMessenger appMessenger) where T : IMessageObject
    {
        RegisterStatic(appMessenger, typeof(T));
    }

    /// <summary>
    /// 注册给定 <paramref name="type"/> 中标记为 <see cref="AppMessageAttribute"/> 的静态方法到 <see cref="AppMessenger"/>。
    /// </summary>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    /// <param name="type">要扫描静态消息方法的类型。</param>
    /// <exception cref="NotSupportedException">当某些不支持的成员类型出现时可能抛出。</exception>
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
    /// 注销指定对象在 <see cref="AppMessenger"/> 中的所有消息订阅。
    /// </summary>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    /// <param name="messageObject">要注销的订阅对象。</param>
    public static void Unregister(this AppMessenger appMessenger, IMessageObject messageObject)
    {
        appMessenger.Remove(messageObject);
    }

    /// <summary>
    /// 注销指定 <paramref name="token"/> 的所有注册项。
    /// </summary>
    /// <param name="appMessenger">目标 <see cref="AppMessenger"/> 实例。</param>
    /// <param name="token">要注销的消息标识符。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="token"/> 为 <see langword="null"/> 或空字符串时抛出。</exception>
    public static void Unregister(this AppMessenger appMessenger, string token)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(token, nameof(token));
        appMessenger.Remove(token);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "源生成器生成的代码在AOT环境中是安全的")]
    private static MethodInfo[] GetInstanceMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "源生成器生成的代码在AOT环境中是安全的")]
    private static MethodInfo[] GetStaticMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }
}