using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示以字符串形式存储的 HTTP 内容。
    /// </summary>
    /// <remarks>
    /// 该类继承自 ReadonlyMemoryHttpContent，用于处理只读的内存中 HTTP 内容。
    /// 它将字符串内容转换为字节数组，并传递给基类以进行处理。
    /// </remarks>
    public class StringHttpContent : ReadonlyMemoryHttpContent
    {
        /// <summary>
        /// 初始化 StringHttpContent 类的新实例。
        /// </summary>
        /// <param name="content">要包含的字符串内容。</param>
        /// <param name="encoding">用于将字符串内容编码为字节数组的编码方式。</param>
        public StringHttpContent(string content, Encoding encoding) : base(encoding.GetBytes(content))
        {
            // 构造函数将字符串内容和编码方式作为参数，将字符串内容转换为字节数组后传递给基类。
        }
    }
}
