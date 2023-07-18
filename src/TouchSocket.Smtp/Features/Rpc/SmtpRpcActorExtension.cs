using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.Smtp.Rpc
{
    /// <summary>
    /// SmtpRpcActorExtension
    /// </summary>
    public static class SmtpRpcActorExtension
    {
        #region DependencyProperty

        /// <summary>
        /// SmtpRpcActor
        /// </summary>
        public static readonly DependencyProperty<ISmtpRpcActor> SmtpRpcActorProperty =
            DependencyProperty<ISmtpRpcActor>.Register("SmtpRpcActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 新创建一个直接向目标地址请求的<see cref="IRpcClient"/>客户端。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="targetId"></param>
        public static IRpcClient CreateIdRpcClient(this ISmtpActorObject client, string targetId)
        {
            return new ITargetSmtpRpcActor(targetId, client.GetSmtpRpcActor());
        }

        /// <summary>
        /// 从<see cref="SmtpActor"/>中获取<see cref="ISmtpRpcActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static ISmtpRpcActor GetSmtpRpcActor(this ISmtpActor smtpActor)
        {
            return smtpActor.GetValue(SmtpRpcActorProperty);
        }

        /// <summary>
        /// 从<see cref="ISmtpActorObject"/>中获取<see cref="ISmtpRpcActor"/>，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ISmtpRpcActor GetSmtpRpcActor(this ISmtpActorObject client)
        {
            var smtpRpcActor = client.SmtpActor.GetSmtpRpcActor();
            if (smtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(smtpRpcActor), TouchSocketSmtpResource.SmtpRpcActorArgumentNull.GetDescription());
            }
            return smtpRpcActor;
        }

        /// <summary>
        /// 从<see cref="ISmtpActorObject"/>中获取继承实现<see cref="ISmtpRpcActor"/>的功能件，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TSmtpRpcActor GetSmtpRpcActor<TSmtpRpcActor>(this ISmtpActorObject client)where TSmtpRpcActor : ISmtpRpcActor
        {
            var smtpRpcActor = client.SmtpActor.GetSmtpRpcActor();
            if (smtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(smtpRpcActor), TouchSocketSmtpResource.SmtpRpcActorArgumentNull.GetDescription());
            }
            return (TSmtpRpcActor)smtpRpcActor;
        }

        /// <summary>
        /// 向<see cref="SmtpActor"/>中设置<see cref="ISmtpRpcActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <param name="smtpRpcActor"></param>
        internal static void SetSmtpRpcActor(this ISmtpActor smtpActor, ISmtpRpcActor smtpRpcActor)
        {
            smtpActor.SetValue(SmtpRpcActorProperty, smtpRpcActor);
        }

        #region 插件扩展

        /// <summary>
        /// 使用SmtpRpc插件
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static SmtpRpcFeature UseSmtpRpc(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<SmtpRpcFeature>();
        }

        /// <summary>
        /// 使用自定义的SmtpRpc插件。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static SmtpRpcFeature UseSmtpRpc<TSmtpRpcFeature>(this IPluginsManager pluginsManager)where TSmtpRpcFeature: SmtpRpcFeature
        {
            return pluginsManager.Add<TSmtpRpcFeature>();
        }
        #endregion 插件扩展
    }
}