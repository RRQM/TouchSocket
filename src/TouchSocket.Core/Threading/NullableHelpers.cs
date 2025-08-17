// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;

namespace TouchSocket.Core;

internal static class NullableHelpers
{
    /// <summary>
    /// Converts a delegate which assumes an argument that is never null into a delegate which might be given a null value,
    /// without adding an explicit null check.
    /// </summary>
    /// <typeparam name="T">The type of argument to be passed to the delegate.</typeparam>
    /// <param name="action">The delegate which, according to the signature, does not expect <see langword="null"/>.</param>
    /// <returns>The exact same referenced delegate, but with a signature that may expect <see langword="null"/>.</returns>
    internal static Action<T> AsNullableArgAction<T>(Action<T> action)
        where T : class
    {
        return action!;
    }

    /// <summary>
    /// Converts a delegate which assumes an argument that is never null into a delegate which might be given a null value,
    /// without adding an explicit null check.
    /// </summary>
    /// <typeparam name="TArg">The type of argument to be passed to the delegate.</typeparam>
    /// <typeparam name="TReturn">The type of value returned from the delegate.</typeparam>
    /// <param name="func">The delegate which, according to the signature, does not expect <see langword="null"/>.</param>
    /// <returns>The exact same referenced delegate, but with a signature that may expect <see langword="null"/>.</returns>
    internal static Func<TArg, TReturn> AsNullableArgFunc<TArg, TReturn>(Func<TArg, TReturn> func)
        where TArg : class
    {
        return func!;
    }
}
