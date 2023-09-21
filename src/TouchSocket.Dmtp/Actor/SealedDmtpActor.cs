namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 密封的<see cref="DmtpActor"/>
    /// </summary>
    public sealed class SealedDmtpActor : DmtpActor
    {
        /// <summary>
        /// 创建一个Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute">是否允许路由</param>
        /// <param name="isReliable">是不是基于可靠协议运行的</param>
        public SealedDmtpActor(bool allowRoute, bool isReliable) : base(allowRoute, isReliable)
        {
        }

        /// <summary>
        /// 创建一个可靠协议的Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute"></param>
        public SealedDmtpActor(bool allowRoute) : this(allowRoute, true)
        {
        }
    }
}