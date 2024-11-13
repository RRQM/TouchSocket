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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TouchSocket.Core
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class ReflectionExtension
    {
        #region MethodInfo

        /// <summary>
        /// 获取方法的确定性名称，即使在重载时，也能区分。
        /// <para>计算规则是：方法名_参数类型名称</para>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetDeterminantName(this MethodInfo methodInfo)
        {
            return GenerateKey(methodInfo);
        }

        private static string GenerateKey(MethodInfo method)
        {
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray(); // 使用类型名称
            return GenerateKey(method.Name, parameterTypes);
        }

        private static string GenerateKey(string methodName, Type[] parameterTypes)
        {
            // 将参数类型名称转换为合法的标识符
            var parameterTypeNames = string.Join("_", parameterTypes.Select(t => StringExtension.MakeIdentifier(t.Name)));
            return $"{StringExtension.MakeIdentifier(methodName)}_{parameterTypeNames}";
        }

        /// <summary>
        /// 获取方法的方法名。主要解决显式实现时函数名称的问题。
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetName(this MethodInfo methodInfo)
        {
            var r = methodInfo.Name.LastIndexOf('.');
            return r < 0 ? methodInfo.Name : methodInfo.Name.Substring(r + 1, methodInfo.Name.Length - r - 1);
        }

        #endregion MethodInfo

        #region ParameterInfo

        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this ParameterInfo parameter)
        {
            return (IEnumerable<string>)DynamicMethodMemberAccessor.Default.GetValue(parameter.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")), "TransformNames");
            //return ((dynamic)parameter.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")))?.TransformNames;
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this PropertyInfo propertyInfo)
        {
            var att = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null || propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        #endregion ParameterInfo

        #region MemberInfo

        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this MemberInfo memberInfo)
        {
            return (IEnumerable<string>)DynamicMethodMemberAccessor.Default.GetValue(memberInfo.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")), "TransformNames");

            //return ((dynamic)memberInfo.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")))?.TransformNames;
        }

        #endregion MemberInfo

        #region FieldInfo

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this FieldInfo fieldInfo)
        {
            var att = fieldInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null || fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        /// <summary>
        /// 检查属性是否可以公开写入
        /// </summary>
        /// <param name="propertyInfo">要检查的属性信息</param>
        /// <returns>如果属性可以公开写入则返回true，否则返回false</returns>
        public static bool CanPublicWrite(this PropertyInfo propertyInfo)
        {
            // 如果属性不支持写操作，则直接返回false
            if (!propertyInfo.CanWrite)
            {
                return false;
            }

            // 如果属性的设置方法为null，则表示不能写入，返回false
            if (propertyInfo.SetMethod == null)
            {
                return false;
            }

            // 如果属性的设置方法是公共的，并且接受一个参数，则认为该属性可以公开写入，返回true
            if (propertyInfo.SetMethod.IsPublic && propertyInfo.SetMethod.GetParameters().Length == 1)
            {
                return true;
            }

            // 如果上述条件都不满足，则返回false
            return false;
        }

        /// <summary>
        /// 判断属性是否可以公共读取
        /// </summary>
        /// <param name="propertyInfo">要检查的属性信息</param>
        /// <returns>如果属性可以公共读取，则为 true；否则为 false</returns>
        public static bool CanPublicRead(this PropertyInfo propertyInfo)
        {
            // 检查属性是否可以读取
            if (!propertyInfo.CanRead)
            {
                return false;
            }

            // 检查属性的获取方法是否存在
            if (propertyInfo.GetMethod == null)
            {
                return false;
            }

            // 判断属性的获取方法是否为公共的且无参数
            if (propertyInfo.GetMethod.IsPublic && propertyInfo.GetMethod.GetParameters().Length == 0)
            {
                return true;
            }

            return false;
        }

        #endregion FieldInfo
    }
}