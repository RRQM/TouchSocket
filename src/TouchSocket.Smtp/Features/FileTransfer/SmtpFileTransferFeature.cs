using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp.FileTransfer
{
    /// <summary>
    /// 能够基于SMTP协议，提供文件传输的能力
    /// </summary>
    public sealed class SmtpFileTransferFeature : PluginBase, ISmtpHandshakedPlugin, ISmtpReceivedPlugin, ISmtpFeature
    {
        private readonly IFileResourceController m_fileResourceController;
        private readonly IPluginsManager m_pluginsManager;

        /// <summary>
        /// 能够基于SMTP协议，提供文件传输的能力
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="container"></param>
        public SmtpFileTransferFeature(IPluginsManager pluginsManager, IContainer container)
        {
            this.m_fileResourceController = container.TryResolve<IFileResourceController>() ?? new FileResourceController();
            this.m_pluginsManager = pluginsManager;
            this.SetProtocolFlags(30);
        }

        /// <inheritdoc/>
        public ushort ReserveProtocolSize => 20;

        /// <inheritdoc/>
        public ushort StartProtocol { get; set; }

        Task ISmtpHandshakedPlugin<ISmtpActorObject>.OnSmtpHandshaked(ISmtpActorObject client, SmtpVerifyEventArgs e)
        {
            var smtpFileTransferActor = new SmtpFileTransferActor(client.SmtpActor)
            {
                FileController = m_fileResourceController,
                OnFileTransfering = OnFileTransfering,
                OnFileTransfered = OnFileTransfered
            };

            smtpFileTransferActor.SetProtocolFlags(this.StartProtocol);
            client.SmtpActor.SetSmtpFileTransferActor(smtpFileTransferActor);

            return e.InvokeNext();
        }

        Task ISmtpReceivedPlugin<ISmtpActorObject>.OnSmtpReceived(ISmtpActorObject client, SmtpMessageEventArgs e)
        {
            if (client.SmtpActor.GetSmtpFileTransferActor() is SmtpFileTransferActor smtpFileTransferActor)
            {
                if (smtpFileTransferActor.InputReceivedData(e.SmtpMessage))
                {
                    e.Handled = true;
                    return EasyTask.CompletedTask;
                }
            }

            return e.InvokeNext();
        }

        /// <summary>
        /// 设置<see cref="SmtpFileTransferFeature"/>的起始协议。
        /// <para>
        /// 默认起始为：30，保留20个协议长度。
        /// </para>
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public SmtpFileTransferFeature SetProtocolFlags(ushort start)
        {
            this.StartProtocol = start;
            return this;
        }

        private void OnFileTransfered(ISmtpActor actor, FileTransferStatusEventArgs e)
        {
            this.m_pluginsManager.Raise(nameof(ISmtpFileTransferedPlugin<ISmtpActorObject>.OnSmtpFileTransfered), actor.Client, e);
        }

        private void OnFileTransfering(ISmtpActor actor, FileOperationEventArgs e)
        {
            this.m_pluginsManager.Raise(nameof(ISmtpFileTransferingPlugin<ISmtpActorObject>.OnSmtpFileTransfering), actor.Client, e);
        }
    }
}