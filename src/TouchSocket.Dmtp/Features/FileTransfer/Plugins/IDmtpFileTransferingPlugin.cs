using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// IDmtpFileTransferPlugin
    /// </summary>
    public interface IDmtpFileTransferingPlugin<TClient> : IPlugin where TClient:IDmtpActorObject
    {
        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnDmtpFileTransfering(TClient client, FileOperationEventArgs e);

    }

    /// <summary>
    /// IDmtpFileTransferingPlugin
    /// </summary>
    public interface IDmtpFileTransferingPlugin: IDmtpFileTransferingPlugin<IDmtpActorObject>
    {

    }
}
