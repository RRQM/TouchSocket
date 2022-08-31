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
using System.Collections.Generic;
using System.Globalization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a JSON constructor.
    /// </summary>
    public partial class JConstructor : JContainer
    {
        private string _name;
        private readonly List<JToken> _values = new List<JToken>();

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => this._values;

        internal override int IndexOfItem(JToken item)
        {
            return this._values.IndexOfReference(item);
        }

        internal override void MergeItem(object content, JsonMergeSettings settings)
        {
            if (!(content is JConstructor c))
            {
                return;
            }

            if (c.Name != null)
            {
                this.Name = c.Name;
            }
            MergeEnumerableContent(this, c, settings);
        }

        /// <summary>
        /// Gets or sets the name of this constructor.
        /// </summary>
        /// <value>The constructor name.</value>
        public string Name
        {
            get => this._name;
            set => this._name = value;
        }

        /// <summary>
        /// Gets the node type for this <see cref="JToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override JTokenType Type => JTokenType.Constructor;

        /// <summary>
        /// Initializes a new instance of the <see cref="JConstructor"/> class.
        /// </summary>
        public JConstructor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JConstructor"/> class from another <see cref="JConstructor"/> object.
        /// </summary>
        /// <param name="other">A <see cref="JConstructor"/> object to copy from.</param>
        public JConstructor(JConstructor other)
            : base(other)
        {
            this._name = other.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name and content.
        /// </summary>
        /// <param name="name">The constructor name.</param>
        /// <param name="content">The contents of the constructor.</param>
        public JConstructor(string name, params object[] content)
            : this(name, (object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name and content.
        /// </summary>
        /// <param name="name">The constructor name.</param>
        /// <param name="content">The contents of the constructor.</param>
        public JConstructor(string name, object content)
            : this(name)
        {
            this.Add(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name.
        /// </summary>
        /// <param name="name">The constructor name.</param>
        public JConstructor(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Constructor name cannot be empty.", nameof(name));
            }

            this._name = name;
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JConstructor c && this._name == c.Name && this.ContentsEqual(c));
        }

        internal override JToken CloneToken()
        {
            return new JConstructor(this);
        }

        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WriteStartConstructor(this._name);

            int count = this._values.Count;
            for (int i = 0; i < count; i++)
            {
                this._values[i].WriteTo(writer, converters);
            }

            writer.WriteEndConstructor();
        }

        /// <summary>
        /// Gets the <see cref="JToken"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="JToken"/> with the specified key.</value>
        public override JToken this[object key]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return this.GetItem((int)key);
            }
            set
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                this.SetItem((int)key, value);
            }
        }

        internal override int GetDeepHashCode()
        {
            return this._name.GetHashCode() ^ this.ContentsHashCode();
        }

        /// <summary>
        /// Loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
        /// <returns>A <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public static new JConstructor Load(JsonReader reader)
        {
            return Load(reader, null);
        }

        /// <summary>
        /// Loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
        /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public static new JConstructor Load(JsonReader reader, JsonLoadSettings settings)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartConstructor)
            {
                throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JConstructor c = new JConstructor((string)reader.Value);
            c.SetLineInfo(reader as IJsonLineInfo, settings);

            c.ReadTokenFrom(reader, settings);

            return c;
        }
    }
}