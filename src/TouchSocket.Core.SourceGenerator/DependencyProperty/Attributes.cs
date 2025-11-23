// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识源生成依赖属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    /*GeneratedCode*/
    internal class GeneratorPropertyAttribute : Attribute
    {
        /// <summary>
        /// 要生成依赖属性的目标类型名称。如果未设置，则表示当前属性所在的类型。
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 生成的属性类型。
        /// </summary>
        public PropertyGenerationOptions GenerationOptions { get; set; } = PropertyGenerationOptions.All;

        /// <summary>
        /// 是否使用Action模式生成Set扩展方法访问器。
        /// </summary>
        public bool ActionMode { get; set; }
    }

    /// <summary>
    /// 依赖属性生成选项。
    /// </summary>
    [Flags]
    enum PropertyGenerationOptions
    {
        /// <summary>
        /// 生成所有访问器。
        /// </summary>
        All = 0,
        /// <summary>
        /// 包含方法形式的 Getter。
        /// </summary>
        IncludeMethodGetter = 1,
        /// <summary>
        /// 包含方法形式的 Setter。
        /// </summary>
        IncludeMethodSetter = 2,
        /// <summary>
        /// 包含属性形式的 Getter。
        /// </summary>
        IncludePropertyGetter = 4,
        /// <summary>
        /// 包含属性形式的 Setter。
        /// </summary>
        IncludePropertySetter = 8
    }
}