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

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;
using System.CodeDom.Compiler;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识源生成<see cref="IPackage"/>的实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    /*GeneratedCode*/
    internal class GeneratorPackageAttribute : Attribute
    {
    }

    /// <summary>
    /// 标识源生成<see cref="IPackage"/>成员的特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    /*GeneratedCode*/
    internal class PackageMemberAttribute : Attribute
    {
        /// <summary>
        /// 生成行为。一般来说，对于非只读、非重写、且同时拥有get，set（可以私有）访问器的属性，会自动生成。
        /// 对于字段，均不会自动生成。所以可以使用该设置，来指示生成器的生成行为。
        /// </summary>
        public PackageBehavior Behavior { get; set; }
        public int Index { get; set; }
        public Type Converter { get; set; }
    }

    /*GeneratedCode*/
    internal enum PackageBehavior : byte
    {
        Ignore,
        Include
    }
}