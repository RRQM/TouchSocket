using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有设置配置的对象接口
    /// </summary>
    public interface ISetupConfigObject:IConfigObject,IPluginObject
    {
        /// <summary>
        /// 配置设置项
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        void Setup(TouchSocketConfig config);
    }
}
