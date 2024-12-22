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

using TouchSocket.Hosting;

namespace TouchSocket.Core
{
    /// <summary>
    /// 为 Registrator 提供扩展方法，使其能够注册 AspNetCoreLogger。
    /// </summary>
    public static class AspNetCoreLoggerExtension
    {
        /// <summary>
        /// 向 IRegistrator 实例中添加 AspNetCoreLogger。
        /// </summary>
        /// <param name="registrator">要扩展的 IRegistrator 实例。</param>
        /// <remarks>
        /// 此方法通过注册 AspNetCoreLogger 实例为日志记录提供支持，该实例在应用程序中作为单例使用。
        /// </remarks>
        public static void AddAspNetCoreLogger(this IRegistrator registrator)
        {
            registrator.RegisterSingleton<ILog, AspNetCoreLogger>();
        }
    }
}