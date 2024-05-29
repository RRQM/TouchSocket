//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 能够基于Dmtp协议，提供文件传输的能力
    /// </summary>
    public sealed class DmtpFileTransferFeature : PluginBase, IDmtpHandshakingPlugin, IDmtpReceivedPlugin, IDmtpFeature
    {
        private readonly IFileResourceController m_fileResourceController;
        private IPluginManager m_pluginManager;

        /// <summary>
        /// 能够基于Dmtp协议，提供文件传输的能力
        /// </summary>
        /// <param name="resolver"></param>
        public DmtpFileTransferFeature(IResolver resolver)
        {
            this.m_fileResourceController = resolver.TryResolve<IFileResourceController>() ?? new FileResourceController();
            this.MaxSmallFileLength = 1024 * 1024;
            this.SetProtocolFlags(30);
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            this.m_pluginManager = pluginManager;
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
        public Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var dmtpFileTransferActor = new DmtpFileTransferActor(client.DmtpActor)
            {
                FileController = this.m_fileResourceController,
                OnFileTransfering = this.OnFileTransfering,
                OnFileTransfered = this.OnFileTransfered,
                RootPath = this.RootPath,
                MaxSmallFileLength = this.MaxSmallFileLength
            };
            dmtpFileTransferActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpFileTransferActor(dmtpFileTransferActor);
            return e.InvokeNext();
        }

        /// <inheritdoc/>
        public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetDmtpFileTransferActor() is DmtpFileTransferActor dmtpFileTransferActor)
            {
                if (await dmtpFileTransferActor.InputReceivedData(e.DmtpMessage).ConfigureFalseAwait())
                {
                    e.Handled = true;
                    return;
                }
            }

            await e.InvokeNext().ConfigureFalseAwait();
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
            return this.m_pluginManager.RaiseAsync(typeof(IDmtpFileTransferedPlugin), actor.Client, e);
        }

        private Task OnFileTransfering(IDmtpActor actor, FileTransferingEventArgs e)
        {
            return this.m_pluginManager.RaiseAsync(typeof(IDmtpFileTransferingPlugin), actor.Client, e);
        }
    }
}