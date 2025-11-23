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

namespace TouchSocket.Core;

internal sealed class DefaultFastSerializerContext : FastSerializerContext
{
    private readonly ConcurrentDictionary<Type, SerializObject> m_instanceCache = new ConcurrentDictionary<Type, SerializObject>();

    /// <summary>
    /// 添加转换器。
    /// </summary>
    /// <param name="type"></param>
    /// <param name="converter"></param>
    public void AddFastBinaryConverter([DynamicallyAccessedMembers(AOT.FastBinaryFormatter)] Type type, IFastBinaryConverter converter)
    {
        base.AddConverter(type, converter);
    }

    [RequiresUnreferencedCode("此方法可能会使用反射构建访问器，与剪裁不兼容。")]
    public override SerializObject GetSerializeObject([DynamicallyAccessedMembers(AOT.FastBinaryFormatter)] Type type)
    {
        var serializObject = base.GetSerializeObject(type);
        if (serializObject != null)
        {
            return serializObject;
        }

        if (type.IsNullableType(out var actualType))
        {
            type = actualType;
        }

        if (this.m_instanceCache.TryGetValue(type, out var instance))
        {
            return instance;
        }

        if (type.IsArray || type.IsClass || type.IsStruct())
        {
            var instanceObject = new SerializObject(type);
            this.m_instanceCache.TryAdd(type, instanceObject);
            return instanceObject;
        }
        return null;
    }
}