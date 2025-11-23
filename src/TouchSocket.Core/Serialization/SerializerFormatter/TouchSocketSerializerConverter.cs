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

namespace TouchSocket.Core;

/// <summary>
/// TouchSocketSerializerConverter 类用于管理和使用多个 ISerializerFormatter 转换器。
/// </summary>
/// <typeparam name="TSource">源数据类型。</typeparam>
/// <typeparam name="TState">状态类型。</typeparam>
public class TouchSocketSerializerConverter<[DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] TSource, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] TState>
{
    private readonly List<ISerializerFormatter<TSource, TState>> m_converters = new List<ISerializerFormatter<TSource, TState>>();

    /// <summary>
    /// 初始化 TouchSocketSerializerConverter 类的新实例。
    /// </summary>
    /// <param name="converters">要添加的 ISerializerFormatter 转换器数组。</param>
    public TouchSocketSerializerConverter(params ISerializerFormatter<TSource, TState>[] converters)
    {
        foreach (var converter in converters)
        {
            this.Add(converter);
        }
    }

    /// <summary>
    /// 初始化 TouchSocketSerializerConverter 类的新实例。
    /// </summary>
    public TouchSocketSerializerConverter()
    { }

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
    /// <param name="state">转换状态</param>
    /// <param name="source">源数据</param>
    /// <param name="targetType">目标类型</param>
    /// <returns>转换后的目标类型对象</returns>
    /// <exception cref="Exception">当无法转换时抛出异常</exception>
    public virtual object Deserialize(TState state, TSource source, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] Type targetType)
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
}