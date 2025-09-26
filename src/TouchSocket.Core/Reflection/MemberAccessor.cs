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

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 动态成员访问器
/// </summary>
/// <typeparam name="T"></typeparam>
public class MemberAccessor<T> : MemberAccessor
{
    /// <summary>
    /// 动态成员访问器
    /// </summary>
    public MemberAccessor() : base(typeof(T))
    {
    }
}

/// <summary>
/// 动态成员访问器
/// </summary>
public class MemberAccessor : IMemberAccessor
{
    private Func<object, string, object> m_getValueDelegate;
    private Dictionary<string, FieldInfo> m_dicFieldInfos;
    private Dictionary<string, PropertyInfo> m_dicProperties;
    private Action<object, string, object> m_setValueDelegate;

    /// <summary>
    /// 动态成员访问器
    /// </summary>
    /// <param name="type"></param>
    public MemberAccessor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
    {
        this.Type = type;
        this.OnGetFieldInfos = (t) => { return t.GetFields(); };
        this.OnGetProperties = (t) => { return t.GetProperties(); };
    }

    /// <summary>
    /// 获取字段
    /// </summary>
    public Func<Type, FieldInfo[]> OnGetFieldInfos { get; set; }

    /// <summary>
    /// 获取属性
    /// </summary>
    public Func<Type, PropertyInfo[]> OnGetProperties { get; set; }

    /// <summary>
    /// 所属类型
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// 构建
    /// </summary>
    public void Build()
    {
        if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
        {
            this.m_dicFieldInfos = this.OnGetFieldInfos.Invoke(this.Type).ToDictionary(a => a.Name);
            this.m_dicProperties = this.OnGetProperties.Invoke(this.Type).ToDictionary(a => a.Name);
        }

        this.m_getValueDelegate = this.GenerateGetValue();
        this.m_setValueDelegate = this.GenerateSetValue();
    }

    /// <inheritdoc/>
    public object GetValue(object instance, string memberName)
    {
        return this.m_getValueDelegate(instance, memberName);
    }

    /// <inheritdoc/>
    public void SetValue(object instance, string memberName, object newValue)
    {
        this.m_setValueDelegate(instance, memberName, newValue);
    }


    private Func<object, string, object> GenerateGetValue()
    {
        if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
        {
            return (obj, key) =>
            {
                return this.m_dicFieldInfos.TryGetValue(key, out var value1)
                    ? value1.GetValue(obj)
                    : this.m_dicProperties.TryGetValue(key, out var value2) ? value2.GetValue(obj) : default;
            };
        }

        var instance = Expression.Parameter(typeof(object), "instance");
        var memberName = Expression.Parameter(typeof(string), "memberName");
        var nameHash = Expression.Variable(typeof(int), "nameHash");
        var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
        var cases = new List<SwitchCase>();
        foreach (var propertyInfo in this.OnGetFieldInfos.Invoke(this.Type))
        {
            try
            {
                var property = Expression.Field(Expression.Convert(instance, this.Type), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }
            catch
            {
            }
        }
        foreach (var propertyInfo in this.OnGetProperties.Invoke(this.Type))
        {
            try
            {
                var property = Expression.Property(Expression.Convert(instance, this.Type), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }
            catch
            {
            }
        }
        if (cases.Count == 0)
        {
            return (a, b) => default;
        }
        var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
        var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

        return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();
    }

    private Action<object, string, object> GenerateSetValue()
    {
        if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
        {
            return (obj, key, value) =>
            {
                if (this.m_dicFieldInfos.TryGetValue(key, out var value1))
                {
                    value1.SetValue(obj, value);
                }
                if (this.m_dicProperties.TryGetValue(key, out var value2))
                {
                    value2.SetValue(obj, value);
                }
            };
        }

        var instance = Expression.Parameter(typeof(object), "instance");
        var memberName = Expression.Parameter(typeof(string), "memberName");
        var newValue = Expression.Parameter(typeof(object), "newValue");
        var nameHash = Expression.Variable(typeof(int), "nameHash");
        var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
        var cases = new List<SwitchCase>();
        foreach (var propertyInfo in this.OnGetFieldInfos.Invoke(this.Type))
        {
            var property = Expression.Field(Expression.Convert(instance, this.Type), propertyInfo.Name);
            var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.FieldType));
            var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

            cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
        }
        foreach (var propertyInfo in this.OnGetProperties(this.Type))
        {
            if (!propertyInfo.CanWrite)
            {
                continue;
            }
            var property = Expression.Property(Expression.Convert(instance, this.Type), propertyInfo.Name);
            var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
            var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

            cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
        }
        if (cases.Count == 0)
        {
            return (a, b, c) => { };
        }
        var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
        var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

        return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
    }
}