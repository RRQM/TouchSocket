using TouchSocket.Core;
using TouchSocket.Dmtp.RouterPackage;

namespace XUnitTestProject.Dmtp._Packages
{
    public class TestPackage : DmtpRouterPackage
    {
        public override int PackageSize => 1024 * 1024 * 2;
        public ByteBlock ByteBlock { get; set; }

        public override void PackageBody(in ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.WriteByteBlock(this.ByteBlock);
        }

        public override void UnpackageBody(in ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.ByteBlock = byteBlock.ReadByteBlock();
        }
    }
}