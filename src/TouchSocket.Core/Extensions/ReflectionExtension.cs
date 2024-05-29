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

namespace TouchSocket.Core
{
    public static class ReflectionExtension
    {
        #region MethodInfo

        /// <summary>
        /// 获取方法的确定性名称，即使在重载时，也能区分。
        /// <para>计算规则是：命名空间.类名.方法名(参数：全名)</para>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetDeterminantName(this MethodInfo methodInfo)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{methodInfo.DeclaringType?.Namespace}.{methodInfo.DeclaringType?.Name}.{methodInfo.GetName()}(");

            foreach (var item in methodInfo.GetParameters())
            {
                stringBuilder.Append(item.ParameterType.FullName);
                if (item != methodInfo.GetParameters().Last())
                {
                    stringBuilder.Append('.');
                }
            }
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取方法的方法名。主要解决显式实现时函数名称的问题。
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetName(this MethodInfo methodInfo)
        {
            var r = methodInfo.Name.LastIndexOf('.');
            if (r < 0)
            {
                return methodInfo.Name;
            }
            return methodInfo.Name.Substring(r + 1, methodInfo.Name.Length - r - 1);
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

        #endregion FieldInfo
    }
}