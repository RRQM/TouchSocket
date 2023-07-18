using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Smtp.FileTransfer
{
    /// <summary>
    /// SmtpFileTransferActorExtension
    /// </summary>
    public static class SmtpFileTransferActorExtension
    {
        #region 插件扩展

        /// <summary>
        /// 使用SmtpFileTransfer插件
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static SmtpFileTransferFeature UseSmtpFileTransfer(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<SmtpFileTransferFeature>();
        }
        #endregion

        #region DependencyProperty

        /// <summary>
        /// SmtpFileTransferActor
        /// </summary>
        public static readonly DependencyProperty<ISmtpFileTransferActor> SmtpFileTransferActorProperty =
            DependencyProperty<ISmtpFileTransferActor>.Register("SmtpFileTransferActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 从<see cref="SmtpActor"/>中获取<see cref="ISmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static ISmtpFileTransferActor GetSmtpFileTransferActor(this ISmtpActor smtpActor)
        {
            return smtpActor.GetValue(SmtpFileTransferActorProperty);
        }

        /// <summary>
        /// 向<see cref="SmtpActor"/>中设置<see cref="SmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <param name="smtpRpcActor"></param>
        internal static void SetSmtpFileTransferActor(this ISmtpActor smtpActor, SmtpFileTransferActor smtpRpcActor)
        {
            smtpActor.SetValue(SmtpFileTransferActorProperty, smtpRpcActor);
        }

        /// <summary>
        /// 从<see cref="ISmtpActorObject"/>中获取能实现文件传输的<see cref="ISmtpFileTransferActor"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ISmtpFileTransferActor GetSmtpFileTransferActor(this ISmtpActorObject client)
        {
            var actor = client.SmtpActor.GetSmtpFileTransferActor();
            if (actor is null)
            {
                throw new ArgumentNullException(nameof(actor), TouchSocketSmtpResource.SmtpFileTransferActorNull.GetDescription());
            }
            else
            {
                return actor;
            }
        }
    }
}