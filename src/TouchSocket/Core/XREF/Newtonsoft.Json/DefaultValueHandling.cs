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

#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion License

using System;
using System.ComponentModel;

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Specifies default value handling options for the <see cref="JsonSerializer"/>.
    /// </summary>
    /// <example>
    ///   <code lang="cs" source="..\Src\TouchSocket.Core.XREF.Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingObject" title="DefaultValueHandling Class" />
    ///   <code lang="cs" source="..\Src\TouchSocket.Core.XREF.Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingExample" title="DefaultValueHandling Ignore Example" />
    /// </example>
    [Flags]
    public enum DefaultValueHandling
    {
        /// <summary>
        /// Include members where the member value is the same as the member's default value when serializing objects.
        /// Included members are written to JSON. Has no effect when deserializing.
        /// </summary>
        Include = 0,

        /// <summary>
        /// Ignore members where the member value is the same as the member's default value when serializing objects
        /// so that it is not written to JSON.
        /// This option will ignore all default values (e.g. <c>null</c> for objects and nullable types; <c>0</c> for integers,
        /// decimals and floating point numbers; and <c>false</c> for booleans). The default value ignored can be changed by
        /// placing the <see cref="DefaultValueAttribute"/> on the property.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Members with a default value but no JSON will be set to their default value when deserializing.
        /// </summary>
        Populate = 2,

        /// <summary>
        /// Ignore members where the member value is the same as the member's default value when serializing objects
        /// and set members to their default value when deserializing.
        /// </summary>
        IgnoreAndPopulate = Ignore | Populate
    }
}