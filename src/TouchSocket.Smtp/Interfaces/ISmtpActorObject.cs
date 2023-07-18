using System.Threading.Tasks;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// 定义包含<see cref="TouchSocket.Smtp.SmtpActor"/>成员的接口对象。
    /// </summary>
    public interface ISmtpActorObject
    {
        /// <summary>
        /// ISmtpActor
        /// </summary>
        ISmtpActor SmtpActor { get; }
    }
}
