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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 动态成员访问器
/// </summary>
public class MemberAccessor
{
    #region static

    private static readonly ConcurrentDictionary<Type, MemberAccessor> s_classAccessors = new();

    /// <summary>
    /// 静态获取成员值
    /// </summary>
    /// <param name="instance">对象实例</param>
    /// <param name="memberName">成员名称</param>
    /// <returns>成员的值</returns>
    [RequiresUnreferencedCode("此方法使用反射构建访问器，与剪裁不兼容。请改用安全的替代方法。")]
    public static object StaticGetValue(object instance, string memberName)
    {
        return FindClassAccessor(instance).GetValue(instance, memberName);
    }

    /// <summary>
    /// 静态设置成员值
    /// </summary>
    /// <param name="instance">对象实例</param>
    /// <param name="memberName">成员名称</param>
    /// <param name="newValue">新值</param>
    [RequiresUnreferencedCode("此方法使用反射构建访问器，与剪裁不兼容。请改用安全的替代方法。")]
    public static void StaticSetValue(object instance, string memberName, object newValue)
    {
        FindClassAccessor(instance).SetValue(instance, memberName, newValue);
    }

    [RequiresUnreferencedCode("此方法使用反射构建访问器，与剪裁不兼容。请改用安全的替代方法。")]
    private static MemberAccessor FindClassAccessor(object instance)
    {
        var typeKey = instance.GetType();
        if (!s_classAccessors.TryGetValue(typeKey, out var classAccessor))
        {
            var memberAccessor = new MemberAccessor(typeKey);
            classAccessor = memberAccessor;
            s_classAccessors.TryAdd(typeKey, classAccessor);
        }
        return classAccessor;
    }

    #endregion static

    private readonly Dictionary<string, FieldInfo> m_dicFieldInfos;
    private readonly Dictionary<string, PropertyInfo> m_dicProperties;
    private readonly Func<object, string, object> m_getValueDelegate;
    private readonly Action<object, string, object> m_setValueDelegate;

    /// <summary>
    /// 初始化<see cref="MemberAccessor"/>实例
    /// </summary>
    /// <param name="type">类型</param>
    public MemberAccessor([DynamicallyAccessedMembers(AOT.MemberAccessor)] Type type)
    {
        this.m_dicFieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(a => a.Name);

        this.m_dicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(a => a.Name);

        this.m_getValueDelegate = this.GenerateGetValue(type);
        this.m_setValueDelegate = this.GenerateSetValue(type);
    }

    /// <summary>
    /// 初始化<see cref="MemberAccessor"/>实例
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="fieldInfos">字段信息字典</param>
    /// <param name="properties">属性信息字典</param>
    public MemberAccessor([DynamicallyAccessedMembers(AOT.MemberAccessor)] Type type, Dictionary<string, FieldInfo> fieldInfos, Dictionary<string, PropertyInfo> properties)
    {
        this.m_dicFieldInfos = fieldInfos;
        this.m_dicProperties = properties;
        this.m_getValueDelegate = this.GenerateGetValue(type);
        this.m_setValueDelegate = this.GenerateSetValue(type);
    }

    /// <summary>
    /// 获取成员值
    /// </summary>
    /// <param name="instance">对象实例</param>
    /// <param name="memberName">成员名称</param>
    /// <returns>成员的值</returns>
    public object GetValue(object instance, string memberName)
    {
        return this.m_getValueDelegate(instance, memberName);
    }

    /// <summary>
    /// 设置成员值
    /// </summary>
    /// <param name="instance">对象实例</param>
    /// <param name="memberName">成员名称</param>
    /// <param name="newValue">新值</param>
    public void SetValue(object instance, string memberName, object newValue)
    {
        this.m_setValueDelegate(instance, memberName, newValue);
    }

    [UnconditionalSuppressMessage("AOT", "IL2026", Justification = "属性一定存在")]
    private Func<object, string, object> GenerateGetValue([DynamicallyAccessedMembers(AOT.MemberAccessor)] Type type)
    {
        try
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in this.m_dicFieldInfos.Values)
            {
                try
                {
                    var property = Expression.Field(Expression.Convert(instance, type), propertyInfo.Name);
                    var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                    cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
                }
                catch
                {
                }
            }
            foreach (var propertyInfo in this.m_dicProperties.Values)
            {
                try
                {
                    var property = Expression.Property(Expression.Convert(instance, type), propertyInfo.Name);
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
        catch
        {
            return (obj, key) =>
            {
                return this.m_dicFieldInfos.TryGetValue(key, out var value1)
                    ? value1.GetValue(obj)
                    : this.m_dicProperties.TryGetValue(key, out var value2) ? value2.GetValue(obj) : default;
            };
        }

    }

    [UnconditionalSuppressMessage("AOT", "IL2026", Justification = "属性一定存在")]
    private Action<object, string, object> GenerateSetValue([DynamicallyAccessedMembers(AOT.MemberAccessor)] Type type)
    {
        try
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in this.m_dicFieldInfos.Values)
            {
                var property = Expression.Field(Expression.Convert(instance, type), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.FieldType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            foreach (var propertyInfo in this.m_dicProperties.Values)
            {
                if (!propertyInfo.CanWrite)
                {
                    continue;
                }
                var property = Expression.Property(Expression.Convert(instance, type), propertyInfo.Name);
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
        catch
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


    }
}