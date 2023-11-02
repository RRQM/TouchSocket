using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有配置的对象接口
    /// </summary>
    public interface IConfigObject:IDependencyObject,ILoggerObject
    {
        /// <summary>
        /// 设置项
        /// </summary>
        TouchSocketConfig Config { get; }
    }
}
