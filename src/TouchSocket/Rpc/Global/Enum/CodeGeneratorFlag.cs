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
        /// 生成同步代码
        /// </summary>
        Sync = 1,

        /// <summary>
        /// 生成异步代码
        /// </summary>
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
        /// 包含接口
        /// </summary>
        IncludeInterface = 16,

        /// <summary>
        /// 包含实例
        /// </summary>
        IncludeInstance = 32,

        /// <summary>
        /// 包含扩展
        /// </summary>
        IncludeExtension = 64
    }
}