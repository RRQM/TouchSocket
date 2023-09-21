using System;
using System.Collections;
using System.Linq.Expressions;

namespace TouchSocket.Core
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// 实例生成
    /// </summary>
    public static class InstanceCreater
    {
        /// <summary>
        /// 根据对象类型创建对象实例
        /// </summary>
        /// <param name="key">对象类型</param>
        /// <returns></returns>
        public static object Create(Type key)
        {
            return Activator.CreateInstance(key);
        }
    }
#else

    /// <summary>
    /// 实例生成
    /// </summary>
    public static class InstanceCreater
    {
        private static readonly Hashtable m_paramCache = Hashtable.Synchronized(new Hashtable());//缓存

        /// <summary>
        /// 根据对象类型创建对象实例
        /// </summary>
        /// <param name="key">对象类型</param>
        /// <returns></returns>
        public static object Create(Type key)
        {
            var value = (Func<object>)m_paramCache[key];
            if (value == null)
            {
                value = CreateInstanceByType(key);
                m_paramCache[key] = value;
            }
            return value();
        }

        private static Func<object> CreateInstanceByType(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type), null).Compile();
        }
    }

#endif
}