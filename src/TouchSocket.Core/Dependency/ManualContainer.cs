using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;

namespace TouchSocket.Core
{
    /// <summary>
    /// 手动IOC容器
    /// </summary>
    public abstract class ManualContainer : IContainer
    {
        private readonly ConcurrentDictionary<string, object> m_singletonInstances = new ConcurrentDictionary<string, object>();

        IEnumerator<DependencyDescriptor> IEnumerable<DependencyDescriptor>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

       /// <summary>
       /// 判断指定的类型是否已在容器中注册。
       /// <para>
       /// 在本容器中，一般均会返回<see langword="true"/>。
       /// </para>
       /// </summary>
       /// <param name="fromType"></param>
       /// <param name="key"></param>
       /// <returns></returns>
        public virtual bool IsRegistered(Type fromType, string key = "")
        {
            return true;
        }

        /// <summary>
        /// 注册描述符。
        /// <para>
        /// 一般情况下，本容器只会处理单例实例模式。
        /// </para>
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        public virtual void Register(DependencyDescriptor descriptor, string key = "")
        {
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    this.m_singletonInstances.AddOrUpdate($"{descriptor.FromType.FullName}{key}", descriptor.ToInstance, (k, v) => descriptor.ToInstance);
                }
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Exception"></exception>
        public object Resolve(Type fromType,string key = "")
        {
            if (fromType.FullName == "TouchSocket.Core.IContainer")
            {
                return this;
            }
            if (this.TryResolve(fromType, out var instance, key))
            {
                return instance;
            }

            throw new Exception($"没有解决容器所需类型：{fromType.FullName}");
        }

        /// <summary>
        /// 默认不实现该功能
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Unregister(DependencyDescriptor descriptor, string key = "")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 尝试解决Ioc容器所需类型。
        /// <para>
        /// 本方法仅实现了在单例实例注册下的获取。
        /// </para>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool TryResolve(Type fromType, out object instance,string key = "")
        {
            if (key.IsNullOrEmpty())
            {
                return this.m_singletonInstances.TryGetValue(fromType.FullName, out instance);
            }
            return this.m_singletonInstances.TryGetValue($"{fromType.FullName}{key}", out instance);
        }
    }
}
