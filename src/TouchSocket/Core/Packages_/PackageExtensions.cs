namespace TouchSocket.Core
{
    /// <summary>
    /// PackageExtensions
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// 打包为字节
        /// </summary>
        /// <param name="packageBase"></param>
        /// <returns></returns>
        public static byte[] PackageAsBytes(this PackageBase packageBase)
        {
            using (ByteBlock byteBlock = new ByteBlock())
            {
                packageBase.Package(byteBlock);
                return byteBlock.ToArray();
            }
        }
    }
}