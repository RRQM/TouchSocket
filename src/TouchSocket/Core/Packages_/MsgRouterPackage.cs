using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 可承载消息的路由包
    /// </summary>
    public class MsgRouterPackage:RouterPackage
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <inheritdoc/>
        public override void PackageBody(ByteBlock byteBlock)
        {
           byteBlock.Write(Message);
        }
        /// <inheritdoc/>
        public override void UnpackageBody(ByteBlock byteBlock)
        {
           this.Message=byteBlock.ReadString();
        }
    }
}
