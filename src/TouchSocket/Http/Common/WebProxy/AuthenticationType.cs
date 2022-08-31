using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 代理身份认证类型
    /// </summary>
    public enum AuthenticationType
    {  /// <summary>
       /// 不允许身份认证
       /// </summary>
        None,
        /// <summary>
        /// 指定摘要身份验证。
        /// </summary>
        Digest = 1,
        /// <summary>
        /// 指定基本身份验证。
        /// </summary>
        Basic = 8,
        /// <summary>
        /// 指定匿名身份验证。
        /// </summary>
        Anonymous = 0x8000
    }
}
