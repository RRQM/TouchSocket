namespace TouchSocket.Core
{
    /// <summary>
    /// 具有目标id和源id的路由包
    /// </summary>
    public class RouterPackage : PackageBase
    {
        /// <summary>
        /// 标识是否路由
        /// </summary>
        public bool Route { get; set; }

        /// <summary>
        /// 源Id
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// 目标Id
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// 打包所有的路由包信息。顺序为：先调用<see cref="PackageRouter(ByteBlock)"/>，然后<see cref="PackageBody(ByteBlock)"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public sealed override void Package(ByteBlock byteBlock)
        {
            PackageRouter(byteBlock);
            PackageBody(byteBlock);
        }

        /// <summary>
        /// 打包数据体。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void PackageBody(ByteBlock byteBlock)
        { 
        
        }

        /// <summary>
        /// 打包路由。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void PackageRouter(ByteBlock byteBlock)
        {
            byteBlock.Write(Route);
            byteBlock.Write(SourceId);
            byteBlock.Write(TargetId);
        }

        /// <summary>
        /// 转换目标和源的id。
        /// </summary>
        public void SwitchId()
        {
            string value = SourceId;
            SourceId = TargetId;
            TargetId = value;
        }

        /// <inheritdoc/>
        public sealed override void Unpackage(ByteBlock byteBlock)
        {
            UnpackageRouter(byteBlock);
            UnpackageBody(byteBlock);
        }

        /// <summary>
        /// 解包数据体。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void UnpackageBody(ByteBlock byteBlock)
        { 
        
        }

        /// <summary>
        /// 只解包路由部分。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void UnpackageRouter(ByteBlock byteBlock)
        {
            Route = byteBlock.ReadBoolean();
            SourceId = byteBlock.ReadString();
            TargetId = byteBlock.ReadString();
        }
    }
}