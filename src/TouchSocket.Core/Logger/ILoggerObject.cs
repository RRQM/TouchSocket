using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有日志记录器的对象接口
    /// </summary>
    public interface ILoggerObject
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }
    }
}
