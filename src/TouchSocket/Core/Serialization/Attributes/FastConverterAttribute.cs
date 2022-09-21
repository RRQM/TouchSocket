using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core.Serialization
{
    /// <summary>
    /// FastConverterAttribute
    /// </summary>
    public class FastConverterAttribute:Attribute
    {
        /// <summary>
        /// FastConverterAttribute
        /// </summary>
        /// <param name="type"></param>
        public FastConverterAttribute(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// 转化器类型。
        /// </summary>
        public Type Type { get;private set; }
    }
}
