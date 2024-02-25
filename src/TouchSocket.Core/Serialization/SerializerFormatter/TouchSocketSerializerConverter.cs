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

namespace TouchSocket.Core
{
    /// <summary>
    /// 序列化转换器
    /// </summary>
    public class TouchSocketSerializerConverter<TSource, TState>
    {
        private readonly List<ISerializerFormatter<TSource, TState>> m_converters = new List<ISerializerFormatter<TSource, TState>>();

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="converter">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(ISerializerFormatter<TSource, TState> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException();
            }
            this.m_converters.RemoveAll(x => x.GetType() == converter.GetType());

            this.m_converters.Add(converter);

            this.m_converters.Sort(delegate (ISerializerFormatter<TSource, TState> x, ISerializerFormatter<TSource, TState> y)
            {
                return x.Order == y.Order ? 0 : x.Order > y.Order ? 1 : -1;
            });
        }

        /// <summary>
        /// 清除所有转化器
        /// </summary>
        public void Clear()
        {
            this.m_converters.Clear();
        }

        /// <summary>
        /// 将源数据转换目标类型对象
        /// </summary>
        /// <param name="state"></param>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public virtual object Deserialize(TState state, TSource source, Type targetType)
        {
            foreach (var item in this.m_converters)
            {
                if (item.TryDeserialize(state, source, targetType, out var result))
                {
                    return result;
                }
            }

            throw new Exception($"{source}无法转换为{targetType}类型。");
        }

        /// <summary>
        /// 将目标类型对象转换源数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual TSource Serialize(TState state, in object target)
        {
            foreach (var item in this.m_converters)
            {
                if (item.TrySerialize(state, target, out var source))
                {
                    return source;
                }
            }

            throw new Exception($"{target}无法转换为{typeof(TSource)}类型。");
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="converter"></param>
        public void Remove(ISerializerFormatter<TSource, TState> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException();
            }
            this.m_converters.Remove(converter);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="type"></param>
        public void Remove(Type type)
        {
            for (var i = this.m_converters.Count - 1; i >= 0; i--)
            {
                var plugin = this.m_converters[i];
                if (plugin.GetType() == type)
                {
                    this.m_converters.RemoveAt(i);
                }
            }
        }
    }
}