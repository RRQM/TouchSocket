using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// Json字节转到对应类
    /// </summary>
    public class JsonBytesToClassSerializerFormatter<TState> : ISerializerFormatter<byte[], TState>
    {
        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool TryDeserialize(TState state, in byte[] source, Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(source), targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual bool TrySerialize(TState state, in object target, out byte[] source)
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
