using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// FastConverterAttribute
    /// </summary>
    public class FastConverterAttribute : Attribute
    {
        /// <summary>
        /// FastConverterAttribute
        /// </summary>
        /// <param name="type"></param>
        public FastConverterAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// 转化器类型。
        /// </summary>
        public Type Type { get; private set; }
    }
}