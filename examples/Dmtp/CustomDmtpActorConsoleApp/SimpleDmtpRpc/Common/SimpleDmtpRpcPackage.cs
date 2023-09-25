using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    class SimpleDmtpRpcPackage : WaitRouterPackage
    {
        protected override bool IncludedRouter => true;

        public string MethodName { get; set; }

        public override void PackageBody(in ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(this.MethodName);
        }

        public override void UnpackageBody(in ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.MethodName = byteBlock.ReadString();
        }
    }

}
