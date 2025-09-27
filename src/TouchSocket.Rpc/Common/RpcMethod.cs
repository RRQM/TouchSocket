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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TouchSocket.Core;

namespace TouchSocket.Rpc;

/// <summary>
/// Rpc函数实例
/// </summary>
public sealed class RpcMethod : Method
{
    private readonly bool[] m_hasFilters = new bool[4];
    private RpcAttribute[] m_rpcAttributes;
    private RpcAttribute[] m_serverRpcAttributes;

    /// <summary>
    /// 实例化一个Rpc调用函数，并在方法声明的类上操作
    /// </summary>
    /// <param name="methodInfo"></param>
    public RpcMethod(MethodInfo methodInfo) : this(methodInfo, methodInfo.DeclaringType, methodInfo.DeclaringType)
    {
    }

    /// <summary>
    /// 实例化一个Rpc调用函数，并在指定类上操作
    /// </summary>
    /// <param name="method"></param>
    /// <param name="serverFromType"></param>
    /// <param name="serverToType"></param>
    public RpcMethod(MethodInfo method, Type serverFromType, Type serverToType) : base(method)
    {
        this.ServerFromType = serverFromType;
        this.ServerToType = serverToType;

        this.Parameters = method.GetParameters().Select(a => new RpcParameter(a)).ToArray();

        //var fromMethodInfos = new Dictionary<string, MethodInfo>();
        //CodeGenerator.GetMethodInfos(this.ServerFromType, ref fromMethodInfos);

        //var toMethodInfos = new Dictionary<string, MethodInfo>();
        //CodeGenerator.GetMethodInfos(this.ServerToType, ref toMethodInfos);

        var attributes = method.GetCustomAttributes<RpcAttribute>();
        if (attributes.Any())
        {
            //if (!toMethodInfos.TryGetValue(CodeGenerator.GetMethodId(method), out var toMethod))
            //{
            //    throw new InvalidOperationException($"没有找到方法{method.Name}的实现");
            //}
            this.ToMethodInfo = CodeGenerator.GetToMethodInfo(method, serverFromType, serverToType);

            this.ParameterNames = this.Parameters.Select(a => a.Name).ToArray();
            this.ParameterTypes = this.Parameters.Select(a => a.Type).ToArray();
        }

        this.PrivateGetFilters();

        if (typeof(ITransientRpcServer).IsAssignableFrom(this.ServerToType) || typeof(IScopedRpcServer).IsAssignableFrom(this.ServerToType))
        {
            this.HasCallContext = true;
        }
        else
        {
            this.HasCallContext = this.Parameters.Where(a => a.IsCallContext).Any();
        }

        var reenterableAttribute = this.PrivateGetReenterableAttribute();
        this.Reenterable = reenterableAttribute?.Reenterable;
    }

    private ReenterableAttribute PrivateGetReenterableAttribute()
    {
        var reenterableAttribute = this.Info.GetCustomAttribute<ReenterableAttribute>();
        if (reenterableAttribute != null)
        {
            return reenterableAttribute;
        }

        reenterableAttribute = this.ServerFromType.GetCustomAttribute<ReenterableAttribute>();
        if (reenterableAttribute != null)
        {
            return reenterableAttribute;
        }

        reenterableAttribute = this.ServerFromType.Assembly.GetCustomAttribute<ReenterableAttribute>();
        if (reenterableAttribute != null)
        {
            return reenterableAttribute;
        }

        return null;
    }

    /// <summary>
    /// 是否可用
    /// </summary>
    [Obsolete("此属性已被弃用", true)]
    public bool IsEnable { get; set; } = true;

    /// <summary>
    /// 参数名集合
    /// </summary>
    public string[] ParameterNames { get; private set; }

    /// <summary>
    /// 参数集合
    /// </summary>
    public RpcParameter[] Parameters { get; private set; }

    /// <summary>
    /// 参数类型集合，已处理out及ref，无参数时为空集合，
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <summary>
    /// 是否包含调用上下文
    /// </summary>
    public bool HasCallContext { get; private set; }

    /// <summary>
    /// 获取常规Rpc参数。
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RpcParameter> GetNormalParameters()
    {
        return this.Parameters.Where(a =>
        {
            if (a.IsFromServices)
            {
                return false;
            }

            if (a.IsCallContext)
            {
                return false;
            }

            if (a.Type.IsInterface)
            {
                return false;
            }

            return true;
        });
    }

    /// <summary>
    /// Rpc属性集合
    /// </summary>
    public RpcAttribute[] RpcAttributes
    {
        get
        {
            this.m_rpcAttributes ??= this.Info.GetCustomAttributes<RpcAttribute>().ToArray();
            return this.m_rpcAttributes;
        }
    }

    /// <summary>
    /// 注册类型
    /// </summary>
    public Type ServerFromType { get; private set; }

    /// <summary>
    /// 获取是否允许重新进入的标志。
    /// </summary>
    /// <value>
    /// 如果允许重新进入，则为 true；如果不允许重新进入，则为 false；如果未定义，则为 null。
    /// </value>
    public bool? Reenterable { get; }

    /// <summary>
    /// Rpc服务属性集合
    /// </summary>
    public RpcAttribute[] ServerRpcAttributes
    {
        get
        {
            this.m_serverRpcAttributes ??= this.ServerFromType.GetCustomAttributes<RpcAttribute>().ToArray();
            return this.m_serverRpcAttributes;
        }
    }

    /// <summary>
    /// 实例类型
    /// </summary>
    public Type ServerToType { get; private set; }

    /// <summary>
    /// 实现方法。
    /// </summary>
    public MethodInfo ToMethodInfo { get; }

    /// <summary>
    /// 获取指定类型属性标签
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetAttribute<T>()
    {
        var attribute = this.GetAttribute(typeof(T));
        return attribute != null ? (T)attribute : default;
    }

    /// <summary>
    /// 获取指定类型属性标签
    /// </summary>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public object GetAttribute(Type attributeType)
    {
        attributeType.GetCustomAttributesData();
        object attribute = this.RpcAttributes.FirstOrDefault((a) => attributeType.IsAssignableFrom(a.GetType()));
        if (attribute != null)
        {
            return attribute;
        }

        attribute = this.ServerRpcAttributes.FirstOrDefault((a) => attributeType.IsAssignableFrom(a.GetType()));
        return attribute ?? default;
    }

    /// <summary>
    /// 描述属性
    /// </summary>
    public string GetDescription()
    {
        return this.Info.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// 筛选器
    /// </summary>
    /// <param name="filters">全局筛选器</param>
    /// <returns></returns>
    public IReadOnlyList<IRpcActionFilter> GetFilters(IReadOnlyList<Type> filters, IResolver resolver)
    {
        if (this.m_hasFilters[0] || this.m_hasFilters[1] || this.m_hasFilters[2] || this.m_hasFilters[3] || filters.Count > 0)
        {
            var actionFilters = new List<IRpcActionFilter>();
            //注册方法
            if (this.m_hasFilters[0])
            {
                foreach (var item in this.Info.GetCustomAttributes(typeof(IRpcActionFilter), false))
                {
                    this.m_hasFilters[0] = true;
                    this.AddActionFilter((IRpcActionFilter)item, ref actionFilters);
                }
            }

            if (this.m_hasFilters[1])
            {
                //实现方法
                if (this.ServerFromType != this.ServerToType)
                {
                    foreach (var item in this.ToMethodInfo.GetCustomAttributes(typeof(IRpcActionFilter), false))
                    {
                        this.m_hasFilters[1] = true;
                        this.AddActionFilter((IRpcActionFilter)item, ref actionFilters);
                    }
                }
            }

            if (this.m_hasFilters[2])
            {
                //注册类
                foreach (var item in this.ServerFromType.GetCustomAttributes(typeof(IRpcActionFilter), false))
                {
                    this.m_hasFilters[2] = true;
                    this.AddActionFilter((IRpcActionFilter)item, ref actionFilters);
                }
            }

            if (this.m_hasFilters[3])
            {
                //实现类
                if (this.ServerFromType != this.ServerToType)
                {
                    foreach (var item in this.ServerToType.GetCustomAttributes(typeof(IRpcActionFilter), false))
                    {
                        this.m_hasFilters[3] = true;
                        this.AddActionFilter((IRpcActionFilter)item, ref actionFilters);
                    }
                }
            }

            //全局筛选器
            foreach (var filterType in filters)
            {
                var filter = resolver.Resolve(filterType);
                this.AddActionFilter(Unsafe.As<object, IRpcActionFilter>(ref filter), ref actionFilters);
            }
            return actionFilters;
        }
        return [];
    }

    private void AddActionFilter(IRpcActionFilter filter, ref List<IRpcActionFilter> filters)
    {
        foreach (var item in filters)
        {
            if (item.GetType() == filter.GetType())
            {
                //同一类型
                return;
            }

            foreach (var item2 in item.MutexAccessTypes)
            {
                if (item2.IsAssignableFrom(filter.GetType()))
                {
                    return;
                }
            }
        }

        filters.Add(filter);
    }

    private void PrivateGetFilters()
    {
        //注册方法
        this.m_hasFilters[0] = this.Info.GetCustomAttributes(typeof(IRpcActionFilter), false).Any();

        //实现方法
        if (this.ServerFromType != this.ServerToType)
        {
            this.m_hasFilters[1] = this.ToMethodInfo.GetCustomAttributes(typeof(IRpcActionFilter), false).Any();
        }

        //注册类
        this.m_hasFilters[2] = this.ServerFromType.GetCustomAttributes(typeof(IRpcActionFilter), false).Any();

        //实现类
        if (this.ServerFromType != this.ServerToType)
        {
            this.m_hasFilters[3] = this.ServerToType.GetCustomAttributes(typeof(IRpcActionFilter), false).Any();
        }
    }
}