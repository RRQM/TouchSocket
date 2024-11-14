using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 定义一个类 JsonMemoryToClassSerializerFormatter，用于将只读内存中的字节序列反序列化为指定的状态类。
    /// 该类实现了 ISerializerFormatter 接口，特化于 ReadOnlyMemory{byte} 类型的输入和 TState 类型的输出。
    /// </summary>
    /// <typeparam name="TState">要反序列化的状态类类型。</typeparam>
    public class JsonMemoryToClassSerializerFormatter<TState> : ISerializerFormatter<ReadOnlyMemory<byte>, TState>
    {
        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        public bool TryDeserialize(TState state, in ReadOnlyMemory<byte> source, Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(source.Span.ToString(Encoding.UTF8), targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TrySerialize(TState state, in object target, out ReadOnlyMemory<byte> source)
        {
            try
            {
                source = JsonConvert.SerializeObject(target, this.JsonSettings).ToUTF8Bytes();
                return true;
            }
            catch (Exception)
            {
                source = null;
                return false;
            }
        }
    }
}
