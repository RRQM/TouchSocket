using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// IDmtpFileTransferedPlugin
    /// </summary>
    public interface IDmtpFileTransferedPlugin<TClient> : IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferedEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnDmtpFileTransfered(TClient client, FileTransferedEventArgs e);
    }

    /// <summary>
    /// IDmtpFileTransferedPlugin
    /// </summary>
    public interface IDmtpFileTransferedPlugin : IDmtpFileTransferedPlugin<IDmtpActorObject>
    {
    }
}