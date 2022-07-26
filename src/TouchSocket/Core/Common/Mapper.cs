using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.Reflection;

namespace TouchSocketPro
{
    /// <summary>
    /// 映射数据
    /// </summary>
    public static class Mapper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Property>> m_typeToProperty = new ConcurrentDictionary<Type, Dictionary<string, Property>>();

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget Map<TTarget>(object source) where TTarget : class, new()
        {
            return (TTarget)Map(source,typeof(TTarget));
        }

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget Map<TSource,TTarget>(TSource source) where TTarget : class, new()
        {
            return (TTarget)Map(source, typeof(TTarget));
        }

        /// <summary>
        /// 简单对象映射
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object Map(object source,Type targetType) 
        {
            return Map(source, Activator.CreateInstance(targetType));
        }

        /// <summary>
        /// 简单对象映射
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object Map(object source, object target)
        {
            if (source is null)
            {
                return default;
            }
            var sourceType = source.GetType();
            if (sourceType.IsPrimitive || sourceType.IsEnum || sourceType == TouchSocket.Core.TouchSocketCoreUtility.stringType)
            {
                return source;
            }
            var sourcePairs = m_typeToProperty.GetOrAdd(sourceType, (k) =>
               {
                   Dictionary<string, Property> pairs = new Dictionary<string, Property>();
                   var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                   foreach (var item in ps)
                   {
                       pairs.Add(item.Name, new Property(item));
                   }
                   return pairs;
               });

            var targetPairs = m_typeToProperty.GetOrAdd(target.GetType(), (k) =>
            {
                Dictionary<string, Property> pairs = new Dictionary<string, Property>();
                var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var item in ps)
                {
                    pairs.Add(item.Name, new Property(item));
                }
                return pairs;
            });
            foreach (var item in targetPairs)
            {
                if (item.Value.CanWrite)
                {
                    if (sourcePairs.TryGetValue(item.Key,out Property property))
                    {
                        if (property.CanRead)
                        {
                            item.Value.SetValue(target,property.GetValue(source));
                        }
                    }
                }
            }
            return target;
        }

     
        /// <summary>
        /// 映射List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T1> MapList<T, T1>(IEnumerable<T> list) where T : class where T1 : class, new()
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            List<T1> result = new List<T1>();
            foreach (var item in list)
            {
                result.Add(Map<T, T1>(item));
            }
            return result;
        }
    }
}
