using System;
using System.Collections.Generic;

namespace RRQMCore.Converter
{
    /// <summary>
    /// 转换器
    /// </summary>
    public class RRQMConverter<TSource>
    {
        private readonly List<IConverter<TSource>> converters = new List<IConverter<TSource>>();

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="converter">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(IConverter<TSource> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var item in this.converters)
            {
                if (item.GetType()==converter.GetType())
                {
                    return;
                }
            }

            this.converters.Add(converter);

            this.converters.Sort(delegate (IConverter<TSource> x, IConverter<TSource> y)
            {
                if (x.Order == y.Order) return 0;
                else if (x.Order > y.Order) return 1;
                else return -1;
            });
        }

        /// <summary>
        /// 清除所有插件
        /// </summary>
        public void Clear()
        {
            this.converters.Clear();
        }

        /// <summary>
        /// 将源数据转换目标类型对象
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object ConvertFrom(TSource source, Type targetType)
        {
            object result;
            foreach (var item in this.converters)
            {
                if (item.TryConvertFrom(source, targetType, out result))
                {
                    return result;
                }
            }

            throw new RRQMException($"{source}无法转换为{targetType}类型。");
        }

        /// <summary>
        /// 将目标类型对象转换源数据
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public TSource ConvertTo(object target)
        {
            foreach (var item in this.converters)
            {
                if (item.TryConvertTo(target,out TSource source))
                {
                    return source;
                }
            }

            throw new RRQMException($"{target}无法转换为{typeof(TSource)}类型。");
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="converter"></param>
        public void Remove(IConverter<TSource> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException();
            }
            this.converters.Remove(converter);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="type"></param>
        public void Remove(Type type)
        {
            for (int i = this.converters.Count - 1; i >= 0; i--)
            {
                IConverter<TSource> plugin = this.converters[i];
                if (plugin.GetType() == type)
                {
                    this.converters.RemoveAt(i);
                }
            }
        }
    }
}