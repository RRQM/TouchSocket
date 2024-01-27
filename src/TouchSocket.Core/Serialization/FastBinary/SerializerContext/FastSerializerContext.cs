using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
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
            this.AddConverter(typeof(string), new StringFastBinaryConverter());
            this.AddConverter(typeof(Version), new VersionFastBinaryConverter());
            this.AddConverter(typeof(ByteBlock), new ByteBlockFastBinaryConverter());
            this.AddConverter(typeof(MemoryStream), new MemoryStreamFastBinaryConverter());
            this.AddConverter(typeof(Guid), new GuidFastBinaryConverter());
            this.AddConverter(typeof(DataTable), new DataTableFastBinaryConverter());
            this.AddConverter(typeof(DataSet), new DataSetFastBinaryConverter());
        }

        /// <summary>
        /// 获取新实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object GetNewInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 获取序列化对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual SerializObject GetSerializObject(Type type)
        {
            if (this.m_instanceCache.TryGetValue(type, out var serializObject))
            {
                return serializObject;
            }

            return null;
        }

        /// <summary>
        /// 添加转换器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        protected void AddConverter([DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
        {
            var serializObject = new SerializObject(type, converter);
            m_instanceCache.AddOrUpdate(type, serializObject);
        }
    }
}