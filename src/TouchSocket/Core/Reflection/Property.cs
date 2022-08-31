//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace TouchSocket.Core.Reflection
{
    /// <summary>
    /// 表示属性
    /// </summary>
    public class Property
    {
        /// <summary>
        /// 获取器
        /// </summary>
        private readonly PropertyGetter m_geter;

        /// <summary>
        /// 设置器
        /// </summary>
        private readonly PropertySetter m_seter;

        /// <summary>
        /// 获取属性名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 获取属性信息
        /// </summary>
        public PropertyInfo Info { get; private set; }

        /// <summary>
        /// 属性
        /// </summary>
        /// <param name="property">属性信息</param>
        public Property(PropertyInfo property)
        {
            this.Name = property.Name;
            this.Info = property;

            if (property.CanRead == true)
            {
                this.CanRead = true;
                this.m_geter = new PropertyGetter(property);
            }
            if (property.CanWrite == true)
            {
                this.CanWrite = true;
                this.m_seter = new PropertySetter(property);
            }
        }

        /// <summary>
        /// 是否可以读取
        /// </summary>
        public bool CanRead { get; private set; }

        /// <summary>
        /// 是否可以写入
        /// </summary>
        public bool CanWrite { get; private set; }

        /// <summary>
        /// 获取属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (this.m_geter == null)
            {
                throw new NotSupportedException();
            }
            return this.m_geter.Invoke(instance);
        }

        /// <summary>
        /// 设置属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="value">值</param>
        /// <exception cref="NotSupportedException"></exception>
        public void SetValue(object instance, object value)
        {
            if (this.m_seter == null)
            {
                throw new NotSupportedException($"{this.Name}不允许赋值");
            }
            this.m_seter.Invoke(instance, value);
        }

        /// <summary>
        /// 类型属性的Setter缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Property[]> cached = new ConcurrentDictionary<Type, Property[]>();

        /// <summary>
        /// 从类型的属性获取属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static Property[] GetProperties(Type type)
        {
            return cached.GetOrAdd(type, t => t.GetProperties().Select(p => new Property(p)).ToArray());
        }
    }
}