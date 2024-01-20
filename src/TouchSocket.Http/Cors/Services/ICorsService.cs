using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 跨域相关的服务类接口
    /// </summary>
    public interface ICorsService
    {
        /// <summary>
        /// 按照策略名称，获取策略
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        CorsPolicy GetPolicy(string name);
    }
}
