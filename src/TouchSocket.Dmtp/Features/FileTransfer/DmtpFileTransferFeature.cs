﻿using System.Threading.Tasks;
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

        /// <inheritdoc/>
        public Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var smtpFileTransferActor = new DmtpFileTransferActor(client.DmtpActor)
            {
                FileController = this.m_fileResourceController,
                OnFileTransfering = this.OnFileTransfering,
                OnFileTransfered = this.OnFileTransfered,
                RootPath = this.RootPath,
                MaxSmallFileLength = this.MaxSmallFileLength
            };
            smtpFileTransferActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpFileTransferActor(smtpFileTransferActor);

            return e.InvokeNext();
        }

        /// <inheritdoc/>
        public Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
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

        private Task OnFileTransfered(IDmtpActor actor, FileTransferedEventArgs e)
        {
            return this.m_pluginsManager.RaiseAsync(nameof(IDmtpFileTransferedPlugin<IDmtpActorObject>.OnDmtpFileTransfered), actor.Client, e);
        }

        private Task OnFileTransfering(IDmtpActor actor, FileTransferingEventArgs e)
        {
            return this.m_pluginsManager.RaiseAsync(nameof(IDmtpFileTransferingPlugin<IDmtpActorObject>.OnDmtpFileTransfering), actor.Client, e);
        }
    }
}