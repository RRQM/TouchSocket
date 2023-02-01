using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 优化平台
    /// </summary>
    [Flags]
    public enum OptimizedPlatforms
    {
        /// <summary>
        /// 无特殊优化
        /// </summary>
        None=0,

        /// <summary>
        /// 针对Unity优化。
        /// 一般来说，在unity2020及以下版本，中执行il2cpp时，需要设置该值。
        /// </summary>
        Unity=1
    }
}
