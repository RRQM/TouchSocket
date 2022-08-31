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
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

#if !HAVE_LINQ
#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class JsonContainerContract : JsonContract
    {
        private JsonContract _itemContract;
        private JsonContract _finalItemContract;

        // will be null for containers that don't have an item type (e.g. IList) or for complex objects
        internal JsonContract ItemContract
        {
            get => this._itemContract;
            set
            {
                this._itemContract = value;
                if (this._itemContract != null)
                {
                    this._finalItemContract = (this._itemContract.UnderlyingType.IsSealed()) ? this._itemContract : null;
                }
                else
                {
                    this._finalItemContract = null;
                }
            }
        }

        // the final (i.e. can't be inherited from like a sealed class or valuetype) item contract
        internal JsonContract FinalItemContract => this._finalItemContract;

        /// <summary>
        /// Gets or sets the default collection items <see cref="JsonConverter" />.
        /// </summary>
        /// <value>The converter.</value>
        public JsonConverter ItemConverter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the collection items preserve object references.
        /// </summary>
        /// <value><c>true</c> if collection items preserve object references; otherwise, <c>false</c>.</value>
        public bool? ItemIsReference { get; set; }

        /// <summary>
        /// Gets or sets the collection item reference loop handling.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

        /// <summary>
        /// Gets or sets the collection item type name handling.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling? ItemTypeNameHandling { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonContainerContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        internal JsonContainerContract(Type underlyingType)
            : base(underlyingType)
        {
            JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(underlyingType);

            if (jsonContainerAttribute != null)
            {
                if (jsonContainerAttribute.ItemConverterType != null)
                {
                    this.ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(
                        jsonContainerAttribute.ItemConverterType,
                        jsonContainerAttribute.ItemConverterParameters);
                }

                this.ItemIsReference = jsonContainerAttribute._itemIsReference;
                this.ItemReferenceLoopHandling = jsonContainerAttribute._itemReferenceLoopHandling;
                this.ItemTypeNameHandling = jsonContainerAttribute._itemTypeNameHandling;
            }
        }
    }
}