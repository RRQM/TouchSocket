using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有关闭动作的对象。
    /// </summary>
    public interface ICloseObject
    {
        /// <summary>
        /// 关闭客户端。
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="Exception"></exception>
        void Close(string msg);
    }
}
