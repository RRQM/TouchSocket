using TouchSocket.Core;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    internal class SimpleDmtpRpcPackage : WaitRouterPackage
    {
        protected override bool IncludedRouter => true;

        public string MethodName { get; set; }


        public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.PackageBody(ref byteBlock);
            byteBlock.Write(this.MethodName);
        }

        public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.UnpackageBody(ref byteBlock);
            this.MethodName = byteBlock.ReadString();
        }

        public void CheckStatus()
        {
            switch (this.Status)
            {
                case 0:
                    throw new TimeoutException();
                case 1: return;
                case 2: throw new Exception("没有找到目标Id");
                case 3: throw new Exception("不允许路由");
                case 4: throw new Exception("没找到Rpc");
                case 5: throw new Exception($"其他异常：{this.Message}");
            }
        }
    }
}