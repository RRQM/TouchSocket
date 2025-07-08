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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 映射数据
/// </summary>

public static partial class Mapper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, Property>> m_typeToProperty = new ConcurrentDictionary<Type, Dictionary<string, Property>>();


    /// <summary>
    /// 将源对象映射到指定目标类型的新实例。
    /// </summary>
    /// <param name="source">源对象，其属性将被映射到目标类型。</param>
    /// <param name="option">映射选项，用于自定义映射行为。</param>
    /// <typeparam name="TTarget">要映射到的目标类型。</typeparam>
    /// <returns>一个新创建的目标类型实例，其属性根据源对象的属性值进行映射。</returns>
    public static TTarget Map<TTarget>(this object source, MapperOption option = default) where TTarget : class, new()
    {
        // 调用泛型方法 Map，将源对象、目标类型和映射选项传递给它
        // 由于目标类型的实例化和类型转换由 Map 方法内部处理，这里直接返回转换后的结果
        return (TTarget)Map(source, typeof(TTarget), option);
    }


    /// <summary>
    /// 扩展方法，用于将对象映射到相同类型的另一个对象。
    /// </summary>
    /// <param name="source">要映射的源对象。</param>
    /// <param name="option">映射选项，用于定制映射行为。</param>
    /// <typeparam name="TTarget">源对象和目标对象的类型。</typeparam>
    /// <returns>返回映射后的目标对象。</returns>
    public static TTarget Map<TTarget>(this TTarget source, MapperOption option = default) where TTarget : class, new()
    {
        // 调用泛型方法 Map，将源对象映射为目标对象
        return (TTarget)Map(source, typeof(TTarget), option);
    }


    /// <summary>
    /// 扩展方法，用于将一个对象映射到另一个类型。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TTarget">目标对象的类型，必须是引用类型且有默认构造函数。</typeparam>
    /// <param name="source">要映射的源对象。</param>
    /// <param name="option">映射选项，用于控制映射行为。</param>
    /// <returns>返回映射后的目标对象实例。</returns>
    public static TTarget Map<TSource, TTarget>(this TSource source, MapperOption option = default) where TTarget : class, new()
    {
        // 调用泛型映射方法，将源对象、目标类型和映射选项传递给它
        // 由于目标类型在运行时才能确定，这里使用反射来动态调用合适的映射方法
        return (TTarget)Map(source, typeof(TTarget), option);
    }


    /// <summary>
    /// 将源对象映射到目标类型的实例。
    /// </summary>
    /// <param name="source">要映射的源对象。</param>
    /// <param name="targetType">目标类型的 <see cref="Type"/>。</param>
    /// <param name="option">映射选项，用于控制映射行为。</param>
    /// <returns>映射后的目标类型实例。</returns>
    public static object Map(this object source, Type targetType, MapperOption option = default)
    {
        // 使用 Activator.CreateInstance 创建目标类型的实例，并将源对象映射到该实例
        return Map(source, Activator.CreateInstance(targetType), option);
    }


    /// <summary>
    /// 将源对象的属性映射到目标对象的属性中。
    /// </summary>
    /// <param name="source">源对象，其属性将被映射。</param>
    /// <param name="target">目标对象，将接收映射的属性值。</param>
    /// <param name="option">映射选项，用于定制映射行为。</param>
    /// <returns>返回映射后的目标对象。</returns>
    public static object Map(this object source, object target, MapperOption option = default)
    {
        if (source is null)
        {
            return default;
        }
        var sourceType = source.GetType();
        if (sourceType.IsPrimitive || sourceType.IsEnum || sourceType == TouchSocketCoreUtility.StringType)
        {
            return source;
        }
        var sourcePairs = m_typeToProperty.GetOrAdd(sourceType, (k) =>
           {
               var pairs = new Dictionary<string, Property>();
               var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
               foreach (var item in ps)
               {
                   pairs.Add(item.Name, new Property(item));
               }
               return pairs;
           });

        var targetPairs = m_typeToProperty.GetOrAdd(target.GetType(), (k) =>
        {
            var pairs = new Dictionary<string, Property>();
            var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var item in ps)
            {
                pairs.Add(item.Name, new Property(item));
            }
            return pairs;
        });

        foreach (var item in sourcePairs)
        {
            if (item.Value.CanRead)
            {
                var pkey = item.Key;
                if (option != null && option.MapperProperties != null && option.MapperProperties.ContainsKey(pkey))
                {
                    pkey = option.MapperProperties[pkey];
                }

                if (option?.IgnoreProperties?.Contains(pkey) == true)
                {
                    continue;
                }
                if (targetPairs.TryGetValue(pkey, out var property))
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(target, item.Value.GetValue(source));
                    }
                }
            }
        }
        return target;
    }


    /// <summary>
    /// 扩展方法，将一个泛型集合中的每个元素映射到另一个泛型类型的新集合。
    /// </summary>
    /// <param name="list">要映射的原始集合。</param>
    /// <param name="option">映射选项，用于自定义映射行为。</param>
    /// <typeparam name="T">原始集合中的元素类型。</typeparam>
    /// <typeparam name="T1">目标集合中的元素类型。</typeparam>
    /// <returns>一个新集合，包含原始集合中每个元素的映射结果。</returns>
    public static IEnumerable<T1> MapList<T, T1>(this IEnumerable<T> list, MapperOption option = default) where T : class where T1 : class, new()
    {
        // 检查输入的集合是否为<see langword="null"/>，如果是，则抛出异常
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        // 初始化结果集合，用于存储映射后的元素
        var result = new List<T1>();
        // 遍历原始集合中的每个元素
        foreach (var item in list)
        {
            // 将当前元素映射到目标类型，并将结果添加到结果集合中
            result.Add(Map<T, T1>(item, option));
        }
        // 返回结果集合
        return result;
    }


    /// <summary>
    /// 将对象集合映射为指定类型的集合。
    /// </summary>
    /// <param name="list">待映射的对象集合。</param>
    /// <param name="option">映射选项。</param>
    /// <typeparam name="T1">目标类型。</typeparam>
    /// <returns>映射后的指定类型的集合。</returns>
    public static IEnumerable<T1> MapList<T1>(this IEnumerable<object> list, MapperOption option = default) where T1 : class, new()
    {
        // 检查输入集合是否为<see langword="null"/>，如果是，则抛出异常
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        // 初始化结果集合
        var result = new List<T1>();
        // 遍历输入集合中的每个对象
        foreach (var item in list)
        {
            // 将当前对象映射为目标类型，并添加到结果集合中
            result.Add(Map<T1>(item, option));
        }
        // 返回结果集合
        return result;
    }
}