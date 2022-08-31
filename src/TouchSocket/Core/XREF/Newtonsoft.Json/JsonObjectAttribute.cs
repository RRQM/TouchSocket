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

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> how to serialize the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class JsonObjectAttribute : JsonContainerAttribute
    {
        private MemberSerialization _memberSerialization = MemberSerialization.OptOut;

        // yuck. can't set nullable properties on an attribute in C#
        // have to use this approach to get an unset default state
        internal Required? _itemRequired;

        internal NullValueHandling? _itemNullValueHandling;

        /// <summary>
        /// Gets or sets the member serialization.
        /// </summary>
        /// <value>The member serialization.</value>
        public MemberSerialization MemberSerialization
        {
            get => this._memberSerialization;
            set => this._memberSerialization = value;
        }

        /// <summary>
        /// Gets or sets how the object's properties with null values are handled during serialization and deserialization.
        /// </summary>
        /// <value>How the object's properties with null values are handled during serialization and deserialization.</value>
        public NullValueHandling ItemNullValueHandling
        {
            get => this._itemNullValueHandling ?? default(NullValueHandling);
            set => this._itemNullValueHandling = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the object's properties are required.
        /// </summary>
        /// <value>
        /// 	A value indicating whether the object's properties are required.
        /// </value>
        public Required ItemRequired
        {
            get => this._itemRequired ?? default(Required);
            set => this._itemRequired = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class.
        /// </summary>
        public JsonObjectAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified member serialization.
        /// </summary>
        /// <param name="memberSerialization">The member serialization.</param>
        public JsonObjectAttribute(MemberSerialization memberSerialization)
        {
            this.MemberSerialization = memberSerialization;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified container Id.
        /// </summary>
        /// <param name="id">The container Id.</param>
        public JsonObjectAttribute(string id)
            : base(id)
        {
        }
    }
}