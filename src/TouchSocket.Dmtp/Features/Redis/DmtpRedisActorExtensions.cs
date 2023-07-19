//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Dmtp.Redis
{
    /// <summary>
    /// DmtpRedisActorExtensions
    /// </summary>
    public static class DmtpRedisActorExtensions
    {
        /// <summary>
        /// 获取或设置RedisActor的注入键。
        /// </summary>
        public static readonly DependencyProperty<IDmtpRedisActor> DmtpRedisActorProperty =
            DependencyProperty<IDmtpRedisActor>.Register("DmtpRedisActor", null);

        /// <summary>
        /// 获取<see cref="IDmtpRedisActor"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IDmtpRedisActor GetDmtpRedisActor(this IDmtpActorObject client)
        {
            var redisClient = client.DmtpActor.GetValue(DmtpRedisActorProperty);
            return redisClient ?? throw new ArgumentException(TouchSocketDmtpResource.RedisActorNull.GetDescription());
        }

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获得<see cref="IDmtpRedisActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        public static IDmtpRedisActor GetDmtpRedisActor(this IDmtpActor smtpActor)
        {
            return smtpActor.GetValue(DmtpRedisActorProperty);
        }

        internal static void SetStmpRedisActor(this IDmtpActor smtpActor, DmtpRedisActor redisClient)
        {
            smtpActor.SetValue(DmtpRedisActorProperty, redisClient);
        }

        /// <summary>
        /// 使用Redis插件。仅：Dmtp端会生效。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static RedisFeature UseDmtpRedis(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<RedisFeature>();
        }
    }
}