using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public class JsonMemoryToClassSerializerFormatter<TState> : ISerializerFormatter<ReadOnlyMemory<byte>, TState>
    {
        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        public int Order { get; set; }

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
