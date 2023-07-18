using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp.FileTransfer
{
    /// <summary>
    /// ISmtpFileTransferPlugin
    /// </summary>
    public interface ISmtpFileTransferingPlugin<TClient> : IPlugin where TClient:ISmtpActorObject
    {
        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSmtpFileTransfering(TClient client, FileOperationEventArgs e);

    }

    /// <summary>
    /// ISmtpFileTransferingPlugin
    /// </summary>
    public interface ISmtpFileTransferingPlugin: ISmtpFileTransferingPlugin<ISmtpActorObject>
    {

    }
}
