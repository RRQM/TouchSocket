using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// String值转换为基础类型。
    /// </summary>
    public class StringToPrimitiveSerializerFormatter<TState> : ISerializerFormatter<string, TState>
    {
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
        public virtual bool TryDeserialize(TState state, in string source, Type targetType, out object target)
        {
            if (targetType.IsPrimitive || targetType == TouchSocketCoreUtility.stringType)
            {
                return StringExtension.TryParseToType(source, targetType, out target);
            }
            target = default;
            return false;
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
            if (target != null)
            {
                var type = target.GetType();
                if (type.IsPrimitive || type == TouchSocketCoreUtility.stringType)
                {
                    source = target.ToString();
                    return true;
                }
            }

            source = null;
            return false;
        }
    }

}
