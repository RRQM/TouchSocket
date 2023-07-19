using System.Threading.Tasks;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 定义包含<see cref="TouchSocket.Dmtp.DmtpActor"/>成员的接口对象。
    /// </summary>
    public interface IDmtpActorObject
    {
        /// <summary>
        /// IDmtpActor
        /// </summary>
        IDmtpActor DmtpActor { get; }
    }
}
