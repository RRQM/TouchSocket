namespace TouchSocket.Core
{
    /// <summary>
    /// 包接口规范
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// 打包。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        void Package(ByteBlock byteBlock);

        /// <summary>
        /// 解包。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        void Unpackage(ByteBlock byteBlock);
    }
}