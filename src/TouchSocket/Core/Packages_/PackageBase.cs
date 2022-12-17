namespace TouchSocket.Core
{
    /// <summary>
    /// PackageBase包结构数据。
    /// </summary>
    public abstract class PackageBase : IPackage
    {
        /// <inheritdoc/>
        public abstract void Package(ByteBlock byteBlock);

        /// <inheritdoc/>
        public abstract void Unpackage(ByteBlock byteBlock);
    }
}