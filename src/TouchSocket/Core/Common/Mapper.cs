using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocketPro
{
    /// <summary>
    /// 映射数据
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T1 Map<T, T1>(T t)where T:class where T1:class,new ()
        {
            if (t is null)
            {
                return default;
            }
            var source = Activator.CreateInstance(typeof(T));
            var result = Activator.CreateInstance(typeof(T1));
            if (source.GetType().Name == "List`1" || result.GetType().Name == "List`1")
            {
                throw new Exception("形参有误！，请使用对象。");
            }
            var tpropertyInfos = source.GetType().GetProperties();
            var t1propertyInfos = result.GetType().GetProperties();
            foreach (var tinfo in tpropertyInfos)
            {
                foreach (var t1info in t1propertyInfos)
                {
                    if (t1info.PropertyType.IsValueType || t1info.PropertyType.Name.StartsWith("String"))
                    {
                        if (tinfo.Name == t1info.Name)
                        {
                            try
                            {
                                object value = tinfo.GetValue(t, null);
                                var property = typeof(T1).GetProperty(tinfo.Name);
                                if (property != null && property.CanWrite && !(value is DBNull))
                                {
                                    property.SetValue(result, value, null);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

            }
            return (T1)result;
        }

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T1 Map<T1>(object t) where T1 : class, new()
        {
            if (t is null)
            {
                return default;
            }
            var result = Activator.CreateInstance(typeof(T1));
            if (t.GetType().Name == "List`1" || result.GetType().Name == "List`1")
            {
                throw new Exception("形参有误！，请使用对象。");
            }
            var tpropertyInfos = t.GetType().GetProperties();
            var t1propertyInfos = result.GetType().GetProperties();
            foreach (var tinfo in tpropertyInfos)
            {
                foreach (var t1info in t1propertyInfos)
                {
                    if (t1info.PropertyType.IsValueType || t1info.PropertyType.Name.StartsWith("String"))
                    {
                        if (tinfo.Name == t1info.Name)
                        {
                            try
                            {
                                object value = tinfo.GetValue(t, null);
                                var property = typeof(T1).GetProperty(tinfo.Name);
                                if (property != null && property.CanWrite && !(value is DBNull))
                                {
                                    property.SetValue(result, value, null);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

            }
            return (T1)result;
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
