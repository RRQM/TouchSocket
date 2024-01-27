using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识Fast序列化成员编号。以此来代替属性、字段名。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class FastMemberAttribute:Attribute
    {
        /// <summary>
        /// 索引号
        /// </summary>
        public byte Index { get;private set; }

        /// <summary>
        /// 标识Fast序列化成员编号。以此来代替属性、字段名。
        /// </summary>
        /// <param name="index">最大支持255个成员</param>
        public FastMemberAttribute(byte index)
        {
            this.Index = index;
        }
    }
}
