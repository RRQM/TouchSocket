using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 能够基于Dmtp协议，提供文件传输的能力
    /// </summary>
    public sealed class DmtpFileTransferFeature : PluginBase, IDmtpHandshakedPlugin, IDmtpReceivedPlugin, IDmtpFeature
    {
        private readonly IFileResourceController m_fileResourceController;
        private readonly IPluginsManager m_pluginsManager;

        /// <summary>
        /// 能够基于Dmtp协议，提供文件传输的能力
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="container"></param>
        public DmtpFileTransferFeature(IPluginsManager pluginsManager, IContainer container)
        {
            this.m_fileResourceController = container.TryResolve<IFileResourceController>() ?? new FileResourceController();
            this.m_pluginsManager = pluginsManager;
            this.MaxSmallFileLength = 1024 * 1024;
            this.SetProtocolFlags(30);
        }

        /// <inheritdoc cref="IDmtpFileTransferActor.MaxSmallFileLength"/>
        public int MaxSmallFileLength { get; set; }

        /// <inheritdoc/>
        public ushort ReserveProtocolSize => 20;

        /// <inheritdoc cref="IDmtpFileTransferActor.RootPath"/>
        public string RootPath { get; set; }

        /// <inheritdoc/>
        public ushort StartProtocol { get; set; }

        Task IDmtpHandshakedPlugin<IDmtpActorObject>.OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var smtpFileTransferActor = new DmtpFileTransferActor(client.DmtpActor)
            {
                FileController = m_fileResourceController,
                OnFileTransfering = OnFileTransfering,
                OnFileTransfered = OnFileTransfered,
                RootPath = this.RootPath,
                MaxSmallFileLength = this.MaxSmallFileLength
            };
            smtpFileTransferActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpFileTransferActor(smtpFileTransferActor);

            return e.InvokeNext();
        }

        Task IDmtpReceivedPlugin<IDmtpActorObject>.OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetDmtpFileTransferActor() is DmtpFileTransferActor smtpFileTransferActor)
            {
                if (smtpFileTransferActor.InputReceivedData(e.DmtpMessage))
                {
                    e.Handled = true;
                    return EasyTask.CompletedTask;
                }
            }

            return e.InvokeNext();
        }

        /// <inheritdoc cref="IDmtpFileTransferActor.MaxSmallFileLength"/>
        public DmtpFileTransferFeature SetMaxSmallFileLength(int maxSmallFileLength)
        {
            this.MaxSmallFileLength = maxSmallFileLength;
            return this;
        }

        /// <summary>
        /// 设置<see cref="DmtpFileTransferFeature"/>的起始协议。
        /// <para>
        /// 默认起始为：30，保留20个协议长度。
        /// </para>
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public DmtpFileTransferFeature SetProtocolFlags(ushort start)
        {
            this.StartProtocol = start;
            return this;
        }

        /// <inheritdoc cref="IDmtpFileTransferActor.RootPath"/>
        public DmtpFileTransferFeature SetRootPath(string rootPath)
        {
            this.RootPath = rootPath;
            return this;
        }

        private void OnFileTransfered(IDmtpActor actor, FileTransferedEventArgs e)
        {
            this.m_pluginsManager.Raise(nameof(IDmtpFileTransferedPlugin<IDmtpActorObject>.OnDmtpFileTransfered), actor.Client, e);
        }

        private void OnFileTransfering(IDmtpActor actor, FileTransferingEventArgs e)
        {
            this.m_pluginsManager.Raise(nameof(IDmtpFileTransferingPlugin<IDmtpActorObject>.OnDmtpFileTransfering), actor.Client, e);
        }
    }
}