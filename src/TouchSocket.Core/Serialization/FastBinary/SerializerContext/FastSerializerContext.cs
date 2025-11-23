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
/// 快速序列化上下文
/// </summary>
public abstract class FastSerializerContext
{
    private readonly Dictionary<Type, SerializObject> m_instanceCache = new Dictionary<Type, SerializObject>();

    /// <summary>
    /// 快速序列化上下文
    /// </summary>
    public FastSerializerContext()
    {
        this.AddConverter(typeof(Version), new VersionFastBinaryConverter());
        this.AddConverter(typeof(ByteBlock), new ByteBlockFastBinaryConverter());
        this.AddConverter(typeof(MemoryStream), new MemoryStreamFastBinaryConverter());
        this.AddConverter(typeof(Guid), new GuidFastBinaryConverter());
        this.AddConverter(typeof(Metadata), new MetadataFastBinaryConverter());
    }

    /// <summary>
    /// 获取新实例
    /// </summary>
    /// <param name="type"></param>
    public virtual object GetNewInstance([DynamicallyAccessedMembers(AOT.FastBinaryFormatter)] Type type)
    {
        return Activator.CreateInstance(type, null);
    }

    /// <summary>
    /// 获取序列化对象
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("此方法可能会使用反射构建访问器，与剪裁不兼容。")]
    public virtual SerializObject GetSerializeObject([DynamicallyAccessedMembers(AOT.FastBinaryFormatter)] Type type)
    {
        return this.m_instanceCache.TryGetValue(type, out var serializObject) ? serializObject : null;
    }

    /// <summary>
    /// 添加转换器
    /// </summary>
    /// <param name="type"></param>
    /// <param name="converter"></param>
    protected void AddConverter([DynamicallyAccessedMembers(AOT.FastBinaryFormatter)] Type type, IFastBinaryConverter converter)
    {
        var serializObject = new SerializObject(type, converter);
        this.m_instanceCache.AddOrUpdate(type, serializObject);
    }
}