using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core
{
    internal sealed class DefaultFastSerializerContext : FastSerializerContext
    {
        private readonly ConcurrentDictionary<Type, SerializObject> m_instanceCache = new ConcurrentDictionary<Type, SerializObject>();

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        public void AddFastBinaryConverter([DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
        {
            base.AddConverter(type, converter);
        }


        public override SerializObject GetSerializObject(Type type)
        {
            var serializObject = base.GetSerializObject(type);
            if (serializObject != null)
            {
                return serializObject;
            }

            if (type.IsNullableType())
            {
                type = type.GetGenericArguments()[0];
            }

            if (m_instanceCache.TryGetValue(type, out var instance))
            {
                return instance;
            }

            if (type.IsArray || type.IsClass || type.IsStruct())
            {
                var instanceObject = new SerializObject(type);
                m_instanceCache.TryAdd(type, instanceObject);
                return instanceObject;
            }
            return null;
        }
    }
}