using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// Json字符串转到对应类
    /// </summary>
    public class JsonStringToClassSerializerFormatter<TState> : ISerializerFormatter<string, TState>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool TryDeserialize(TState state, in string source, Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(source, targetType, this.JsonSettings);
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
        public virtual bool TrySerialize(TState state, in object target, out string source)
        {
            try
            {
                source = JsonConvert.SerializeObject(target, this.JsonSettings);
                return true;
            }
            catch
            {
                source = null;
                return false;
            }
        }
    }
}
