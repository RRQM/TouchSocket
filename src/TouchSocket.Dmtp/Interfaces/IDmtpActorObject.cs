namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 定义包含<see cref="TouchSocket.Dmtp.IDmtpActor"/>成员的接口对象。
    /// </summary>
    public interface IDmtpActorObject
    {
        /// <summary>
        /// 提供Dmtp协议的最基础功能件
        /// </summary>
        IDmtpActor DmtpActor { get; }
    }
}