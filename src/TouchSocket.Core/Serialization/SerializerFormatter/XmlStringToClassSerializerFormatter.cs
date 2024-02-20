using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// Xml字符串转换器
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class XmlStringToClassSerializerFormatter<TState> : ISerializerFormatter<string, TState>
    {
        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        public virtual bool TryDeserialize(TState state, in string source, Type targetType, out object target)
        {
            try
            {
                target = SerializeConvert.XmlDeserializeFromString(source, targetType);
                return true;
            }
            catch
            {
                target = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual bool TrySerialize(TState state, in object target, out string source)
        {
            try
            {
                source = SerializeConvert.XmlSerializeToString(target);
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