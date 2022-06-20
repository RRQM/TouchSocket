//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RRQMCore.Dependency
{

    /// <summary>
    /// IOC容器
    /// </summary>
    public class Container : IContainer
    {
        private readonly Dictionary<string, DependencyDescriptor> registrations = new Dictionary<string, DependencyDescriptor>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        public void Register(DependencyDescriptor descriptor, string key = "")
        {
            string k = $"{descriptor.FromType.FullName}{key}";
            registrations.AddOrUpdate(k, descriptor);
        }

        private IScopedContainer GetScopedContainer()
        {
            Container container = new Container();
            foreach (var item in this.registrations)
            {
                if (item.Value.Lifetime == Lifetime.Scoped)
                {
                    container.registrations.AddOrUpdate(item.Key, new DependencyDescriptor(item.Value.FromType, item.Value.ToType, Lifetime.Singleton));
                }
                else
                {
                    container.registrations.AddOrUpdate(item.Key, item.Value);
                }

            }
            return new ScopedContainer(container);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="ps"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Resolve(Type fromType, object[] ps = null, string key = "")
        {
            if (fromType == typeof(IScopedContainer))
            {
                return GetScopedContainer();
            }

            if (fromType.IsGenericType)
            {
                Type type = fromType.GetGenericTypeDefinition();
                string k = $"{type.FullName}{key}";
                if (registrations.TryGetValue(k, out DependencyDescriptor descriptor))
                {
                    if (descriptor.Lifetime == Lifetime.Singleton)
                    {
                        if (descriptor.ToInstance != null)
                        {
                            return descriptor.ToInstance;
                        }
                        if (descriptor.ToType.IsGenericType)
                        {
                            return descriptor.ToInstance = this.Create(descriptor.ToType.MakeGenericType(fromType.GetGenericArguments()), ps);
                        }
                        else
                        {
                            return descriptor.ToInstance = this.Create(descriptor.ToType, ps);
                        }

                    }
                    if (descriptor.ToType.IsGenericType)
                    {
                        return this.Create(descriptor.ToType.MakeGenericType(fromType.GetGenericArguments()), ps);
                    }
                    else
                    {
                        return this.Create(descriptor.ToType, ps);
                    }
                }
                else
                {
                    return default;
                }
            }
            else
            {
                string k = $"{fromType.FullName}{key}";
                if (registrations.TryGetValue(k, out DependencyDescriptor descriptor))
                {
                    if (descriptor.Lifetime == Lifetime.Singleton)
                    {
                        if (descriptor.ToInstance != null)
                        {
                            return descriptor.ToInstance;
                        }
                        return descriptor.ToInstance = this.Create(descriptor.ToType, ps);
                    }
                    return this.Create(descriptor.ToType, ps);
                }
                else if (fromType.IsPrimitive || fromType == typeof(string))
                {
                    return default;
                }
                else if (fromType.IsClass && !fromType.IsAbstract)
                {
                    return this.Create(fromType, ps);
                }
                else
                {
                    return default;
                }
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="ops"></param>
        /// <returns></returns>
        private object Create(Type toType, object[] ops)
        {
            ConstructorInfo ctor = toType.GetConstructors().FirstOrDefault(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
            if (ctor is null)
            {
                //如果没有被特性标记，那就取构造函数参数最多的作为注入目标
                ctor = toType.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
                if (ctor is null)
                {
                    throw new RRQMException($"没有找到类型{toType.FullName}的公共构造函数。");
                }
            }
            else
            {
                if (ops == null)
                {
                    ops = ctor.GetCustomAttribute<DependencyInjectAttribute>().Ps;
                }
            }

            DependencyTypeAttribute dependencyTypeAttribute = null;
            if (toType.IsDefined(typeof(DependencyTypeAttribute), true))
            {
                dependencyTypeAttribute = toType.GetCustomAttribute<DependencyTypeAttribute>();
            }


            var parameters = ctor.GetParameters();
            object[] ps = new object[parameters.Length];

            if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Constructor))
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (ops != null && ops.Length - 1 >= i)
                    {
                        ps[i] = ops[i];
                    }
                    else
                    {
                        if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                        {
                            if (parameters[i].HasDefaultValue)
                            {
                                ps[i] = parameters[i].DefaultValue;
                            }
                            else
                            {
                                ps[i] = default;
                            }
                        }
                        else
                        {
                            if (parameters[i].IsDefined(typeof(DependencyParamterInjectAttribute), true))
                            {
                                DependencyParamterInjectAttribute attribute = parameters[i].GetCustomAttribute<DependencyParamterInjectAttribute>();
                                ps[i] = this.Resolve(parameters[i].ParameterType, attribute.Ps, attribute.Key);
                            }
                            else
                            {
                                ps[i] = this.Resolve(parameters[i].ParameterType, null);
                            }

                        }
                    }

                }
            }
            object instance = Activator.CreateInstance(toType, ps);

            if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Property))
            {
                var propetys = toType.GetProperties().Where(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
                foreach (var item in propetys)
                {
                    if (item.CanWrite)
                    {
                        object obj;
                        if (item.IsDefined(typeof(DependencyParamterInjectAttribute), true))
                        {
                            DependencyParamterInjectAttribute attribute = item.GetCustomAttribute<DependencyParamterInjectAttribute>();
                            obj = this.Resolve(item.PropertyType, attribute.Ps, attribute.Key);
                        }
                        else
                        {
                            obj = this.Resolve(item.PropertyType, null);
                        }
                        item.SetValue(instance, obj);
                    }
                }
            }

            if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Method))
            {
                var methods = toType.GetMethods().Where(x => x.IsDefined(typeof(DependencyInjectAttribute), true)).ToList();
                foreach (var item in methods)
                {
                    parameters = item.GetParameters();
                    ops = item.GetCustomAttribute<DependencyInjectAttribute>().Ps;
                    ps = new object[parameters.Length];
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (ops != null && ops.Length - 1 >= i)
                        {
                            ps[i] = ops[i];
                        }
                        else
                        {
                            if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                            {
                                if (parameters[i].HasDefaultValue)
                                {
                                    ps[i] = parameters[i].DefaultValue;
                                }
                                else
                                {
                                    ps[i] = default;
                                }
                            }
                            else
                            {
                                if (parameters[i].IsDefined(typeof(DependencyParamterInjectAttribute), true))
                                {
                                    DependencyParamterInjectAttribute attribute = parameters[i].GetCustomAttribute<DependencyParamterInjectAttribute>();
                                    ps[i] = this.Resolve(parameters[i].ParameterType, attribute.Ps, attribute.Key);
                                }
                                else
                                {
                                    ps[i] = this.Resolve(parameters[i].ParameterType, null);
                                }

                            }
                        }
                    }
                    item.Invoke(instance, ps);
                }
            }
            return instance;
        }
    }
}
