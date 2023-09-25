using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    static class SimpleDmtpRpcExtension
    {
        #region DependencyProperty

        /// <summary>
        /// SimpleDmtpRpcActor
        /// </summary>
        public static readonly DependencyProperty<ISimpleDmtpRpcActor> SimpleDmtpRpcActorProperty =
            DependencyProperty<ISimpleDmtpRpcActor>.Register("SimpleDmtpRpcActor", default);

        #endregion DependencyProperty

        #region 插件扩展

        /// <summary>
        /// 使用SimpleDmtpRpc插件
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static SimpleDmtpRpcFeature UseSimpleDmtpRpc(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<SimpleDmtpRpcFeature>();
        }
        #endregion 插件扩展

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获取<see cref="ISimpleDmtpRpcActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static ISimpleDmtpRpcActor GetSimpleDmtpRpcActor(this IDmtpActor smtpActor)
        {
            return smtpActor.GetValue(SimpleDmtpRpcActorProperty);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取<see cref="ISimpleDmtpRpcActor"/>，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ISimpleDmtpRpcActor GetSimpleDmtpRpcActor(this IDmtpActorObject client)
        {
            var smtpRpcActor = client.DmtpActor.GetSimpleDmtpRpcActor();
            if (smtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(smtpRpcActor), "SimpleRpcAcotr为空，请检查是否已启用UseSimpleDmtpRpc");
            }
            return smtpRpcActor;
        }

        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="ISimpleDmtpRpcActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <param name="smtpRpcActor"></param>
        internal static void SetSimpleDmtpRpcActor(this IDmtpActor smtpActor, ISimpleDmtpRpcActor smtpRpcActor)
        {
            smtpActor.SetValue(SimpleDmtpRpcActorProperty, smtpRpcActor);
        }
    }

}
