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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 代码生成标识
    /// </summary>
    [Flags]
    public enum CodeGeneratorFlag
    {
        /// <summary>
        /// 生成同步代码（源代码生成无效）
        /// </summary>
        [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
        Sync = 1,

        /// <summary>
        /// 生成异步代码（源代码生成无效）
        /// </summary>
        [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
        Async = 2,

        /// <summary>
        /// 生成扩展同步代码
        /// </summary>
        ExtensionSync = 4,

        /// <summary>
        /// 生成扩展异步代码
        /// </summary>
        ExtensionAsync = 8,

        /// <summary>
        /// 包含接口（源代码生成无效）
        /// </summary>
        [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
        IncludeInterface = 16,

        /// <summary>
        /// 包含实例（源代码生成无效）
        /// </summary>
        [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
        IncludeInstance = 32,

        /// <summary>
        /// 包含扩展（源代码生成无效）
        /// </summary>
        [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
        IncludeExtension = 64,

        /// <summary>
        /// 生成实例类同步代码（源代码生成无效）
        /// </summary>
        InstanceSync = 128,

        /// <summary>
        /// 生成实例类异步代码（源代码生成无效）
        /// </summary>
        InstanceAsync = 256,

        /// <summary>
        /// 生成接口同步代码
        /// </summary>
        InterfaceSync = 512,

        /// <summary>
        /// 生成接口异步代码
        /// </summary>
        InterfaceAsync = 1024,
    }
}