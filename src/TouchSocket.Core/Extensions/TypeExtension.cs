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

using System;
using System.Linq;
using System.Reflection;

namespace TouchSocket.Core
{
    /// <summary>
    /// TypeExtension
    /// </summary>
    public static class TypeExtension
    {
        #region Type扩展

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetRefOutType(this Type type)
        {
            return type.IsByRef ? type.GetElementType() : type;
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object GetDefault(this Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        /// <summary>
        /// 判断是否为静态类。
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStatic(this Type targetType)
        {
            return targetType.IsAbstract && targetType.IsSealed;
        }

        /// <summary>
        /// 判断为结构体
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type targetType)
        {
            return !targetType.IsPrimitive && !targetType.IsClass && !targetType.IsEnum && targetType.IsValueType;
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType));
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this PropertyInfo propertyInfo)
        {
            var att = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null
                ? true
                : propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this FieldInfo fieldInfo)
        {
            var att = fieldInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null
                ? true
                : fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        /// <summary>
        /// 判断该类型是否为值元组类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsValueTuple(this Type theType)
        {
            return theType.IsValueType &&
                 theType.IsGenericType &&
                 theType.FullName.StartsWith("System.ValueTuple");
        }

        #endregion Type扩展
    }
}