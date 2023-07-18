//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Smtp.Redis
{
    /// <summary>
    /// SmtpRedisActorExtensions
    /// </summary>
    public static class SmtpRedisActorExtensions
    {
        /// <summary>
        /// 获取或设置RedisActor的注入键。
        /// </summary>
        public static readonly DependencyProperty<ISmtpRedisActor> SmtpRedisActorProperty =
            DependencyProperty<ISmtpRedisActor>.Register("SmtpRedisActor", null);

        /// <summary>
        /// 获取<see cref="ISmtpRedisActor"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ISmtpRedisActor GetSmtpRedisActor(this ISmtpActorObject client)
        {
            var redisClient = client.SmtpActor.GetValue(SmtpRedisActorProperty);
            return redisClient ?? throw new ArgumentException(TouchSocketSmtpResource.RedisActorNull.GetDescription());
        }

        /// <summary>
        /// 从<see cref="SmtpActor"/>中获得<see cref="ISmtpRedisActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static ISmtpRedisActor GetSmtpRedisActor(this ISmtpActor smtpActor)
        {
            return smtpActor.GetValue(SmtpRedisActorProperty);
        }

        internal static void SetStmpRedisActor(this ISmtpActor smtpActor, SmtpRedisActor redisClient)
        {
            smtpActor.SetValue(SmtpRedisActorProperty, redisClient);
        }

        /// <summary>
        /// 使用Redis插件。仅：Smtp端会生效。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static RedisFeature UseSmtpRedis(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<RedisFeature>();
        }
    }
}