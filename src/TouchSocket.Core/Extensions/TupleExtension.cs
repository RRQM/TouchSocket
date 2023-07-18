using System;
using System.Collections.Generic;
using System.Reflection;

namespace TouchSocket.Core
{
    /// <summary>
    /// 元组扩展
    /// </summary>
    public static class TupleExtension
    {
        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this ParameterInfo parameter)
        {
            return ((dynamic)parameter.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")))?.TransformNames;
        }

        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this MemberInfo memberInfo)
        {
            return ((dynamic)memberInfo.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")))?.TransformNames;
        }
    }
}
