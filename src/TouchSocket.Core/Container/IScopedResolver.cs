using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 定义一个作用域解析器接口。
    /// </summary>
    public interface IScopedResolver : IDisposableObject
    {
        /// <summary>
        /// 获取解析器实例。
        /// </summary>
        IResolver Resolver { get; }
    }
}
