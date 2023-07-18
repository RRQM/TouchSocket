using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp.FileTransfer
{
    /// <summary>
    /// ISmtpFileTransferedPlugin
    /// </summary>
    public interface ISmtpFileTransferedPlugin<TClient> : IPlugin where TClient: ISmtpActorObject
    {
        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSmtpFileTransfered(TClient client, FileTransferStatusEventArgs e);

    }

    /// <summary>
    /// ISmtpFileTransferedPlugin
    /// </summary>
    public interface ISmtpFileTransferedPlugin: ISmtpFileTransferedPlugin<ISmtpActorObject>
    { 
    
    }
}
