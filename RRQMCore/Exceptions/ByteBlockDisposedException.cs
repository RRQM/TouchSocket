using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Exceptions
{
    /// <summary>
    /// 内存块已释放
    /// </summary>
    [Serializable]
    public class ByteBlockDisposedException : RRQMException
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ByteBlockDisposedException() { }
        public ByteBlockDisposedException(string message) : base(message) { }
        public ByteBlockDisposedException(string message, Exception inner) : base(message, inner) { }
        protected ByteBlockDisposedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }
}
