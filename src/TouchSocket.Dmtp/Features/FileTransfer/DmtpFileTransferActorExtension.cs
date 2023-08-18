using System;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// DmtpFileTransferActorExtension
    /// </summary>
    public static class DmtpFileTransferActorExtension
    {
        #region 插件扩展

        /// <summary>
        /// 使用DmtpFileTransfer插件
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static DmtpFileTransferFeature UseDmtpFileTransfer(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<DmtpFileTransferFeature>();
        }
        #endregion

        #region DependencyProperty

        /// <summary>
        /// DmtpFileTransferActor
        /// </summary>
        public static readonly DependencyProperty<IDmtpFileTransferActor> DmtpFileTransferActorProperty =
            DependencyProperty<IDmtpFileTransferActor>.Register("DmtpFileTransferActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获取<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActor smtpActor)
        {
            return smtpActor.GetValue(DmtpFileTransferActorProperty);
        }

        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="DmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <param name="smtpRpcActor"></param>
        internal static void SetDmtpFileTransferActor(this IDmtpActor smtpActor, DmtpFileTransferActor smtpRpcActor)
        {
            smtpActor.SetValue(DmtpFileTransferActorProperty, smtpRpcActor);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取能实现文件传输的<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActorObject client)
        {
            var actor = client.DmtpActor.GetDmtpFileTransferActor();
            if (actor is null)
            {
                throw new ArgumentNullException(nameof(actor), TouchSocketDmtpResource.DmtpFileTransferActorNull.GetDescription());
            }
            else
            {
                return actor;
            }
        }
    }
}