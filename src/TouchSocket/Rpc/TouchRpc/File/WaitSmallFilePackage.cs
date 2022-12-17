using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    internal class WaitSmallFilePackage : WaitRouterPackage
    {
        public byte[] Data { get; set; }
        public RemoteFileInfo FileInfo { get; set; }
        public int Len { get; set; }
        public Metadata Metadata { get; set; }
        public string Path { get; set; }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(Path);
            byteBlock.WritePackage(Metadata);
            byteBlock.WritePackage(FileInfo);
            byteBlock.WriteBytesPackage(Data, 0, Len);
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            Path = byteBlock.ReadString();
            Metadata = byteBlock.ReadPackage<Metadata>();
            FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
            Data = byteBlock.ReadBytesPackage();
        }
    }
}
