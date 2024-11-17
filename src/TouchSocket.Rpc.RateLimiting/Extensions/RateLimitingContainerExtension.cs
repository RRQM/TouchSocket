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

using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// 提供扩展方法以方便注册限流策略。
    /// </summary>
    public static class RateLimitingContainerExtension
    {
        /// <summary>
        /// 向注册器中注册限流策略。
        /// </summary>
        /// <param name="registrator">需要注册限流策略的注册器。</param>
        /// <param name="action">用于配置限流策略选项的委托。</param>
        /// <returns>返回配置了限流策略的注册器。</returns>
        public static IRegistrator AddRateLimiter(this IRegistrator registrator, Action<RateLimiterOptions> action)
        {
            // 创建一个新的RateLimiterOptions实例以供配置。
            var options = new RateLimiterOptions();
            // 使用提供的委托配置RateLimiterOptions实例。
            action.Invoke(options);
            // 将配置好的限流服务注册到注册器中。
            return registrator.RegisterSingleton<IRateLimitService>(new RateLimitService(options));
        }
    }
}