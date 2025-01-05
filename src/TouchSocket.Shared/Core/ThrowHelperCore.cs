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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    internal static partial class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowMessageRegisteredException(string tokenString)
        {
            throw new MessageRegisteredException(TouchSocketCoreResource.TokenExisted.Format(tokenString));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowMessageNotFoundException(string tokenString)
        {
            throw new MessageNotFoundException(TouchSocketCoreResource.MessageNotFound.Format(tokenString));
        }
        #region NotSupportedException
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedException(string message)
        {
            throw CreateNotSupportedException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException CreateNotSupportedException(string message)
        {
            return new NotSupportedException(message);
        }
        #endregion


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException_BetweenAnd(string name, long actualValue, long min, long max)
        {
            throw new ArgumentOutOfRangeException(name, TouchSocketCoreResource.ValueBetweenAnd.Format(name, actualValue, min, max));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException_LessThan(string name, long actualValue, long min)
        {
            throw new ArgumentOutOfRangeException(name, TouchSocketCoreResource.ValueLessThan.Format(name, actualValue, min));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException_MoreThan(string name, long actualValue, long max)
        {
            throw new ArgumentOutOfRangeException(name, TouchSocketCoreResource.ValueMoreThan.Format(name, actualValue, max));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTimeoutException()
        {
            throw new TimeoutException(TouchSocketCoreResource.OperationOvertime);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowOperationCanceledException()
        {
            throw new OperationCanceledException(TouchSocketCoreResource.OperationCanceled);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(string name)
        {
            throw new ArgumentNullException(TouchSocketCoreResource.ArgumentIsNull.Format(name));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(string name, string msg)
        {
            throw new ArgumentNullException(name, msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullExceptionIfStringIsNullOrEmpty(string stringValue, string name)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentNullException(TouchSocketCoreResource.ArgumentIsNull.Format(name));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T ThrowArgumentNullExceptionIf<T>(T obj, string objectName) where T : class
        {
            return obj ?? throw new ArgumentNullException(TouchSocketCoreResource.ArgumentIsNull.Format(objectName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowException(string message)
        {
            throw new Exception(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowUnknownErrorException()
        {
            throw new UnknownErrorException();
        }

        #region InvalidEnumArgumentException
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidEnumArgumentException(Enum @enum)
        {
            throw CreateInvalidEnumArgumentException(@enum);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidEnumArgumentException CreateInvalidEnumArgumentException(Enum @enum)
        {
            return new InvalidEnumArgumentException(TouchSocketCoreResource.InvalidEnum.Format(@enum.GetType(), @enum));
        }
        #endregion


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectDisposedException(object obj)
        {
            throw new ObjectDisposedException(TouchSocketCoreResource.ObjectDisposed.Format(obj.GetType().FullName, obj.GetHashCode()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectDisposedExceptionIf(IDisposableObject disposableObject)
        {
            if (disposableObject.DisposedValue)
            {
                throw new ObjectDisposedException(TouchSocketCoreResource.ObjectDisposed.Format(disposableObject.GetType().FullName, disposableObject.GetHashCode()));
            }
        }
    }
}