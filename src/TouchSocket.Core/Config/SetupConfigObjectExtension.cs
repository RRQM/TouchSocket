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

namespace TouchSocket.Core
{
    /// <summary>
    /// 提供配置对象的扩展方法。
    /// </summary>
    public static class SetupConfigObjectExtension
    {
        /// <summary>
        /// 同步配置方法。
        /// 使用给定的配置对象进行设置。
        /// </summary>
        /// <param name="setupConfigObject">要配置的配置对象。</param>
        /// <param name="config">用于配置的配置对象。</param>
        public static void Setup(this ISetupConfigObject setupConfigObject, TouchSocketConfig config)
        {
            // 调用异步配置方法并忽略结果。这里解释了为什么需要忽略结果：可能是因为我们不关注异步操作的完成状态，或者我们已经处理了异步操作可能返回的所有必要结果。
            setupConfigObject.SetupAsync(config).GetFalseAwaitResult();
        }
    }
}