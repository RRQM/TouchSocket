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

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
    /// </summary>
    public class JTokenReader : JsonReader, IJsonLineInfo
    {
        private readonly JToken _root;
        private string _initialPath;
        private JToken _parent;
        private JToken _current;

        /// <summary>
        /// Gets the <see cref="JToken"/> at the reader's current position.
        /// </summary>
        public JToken CurrentToken => this._current;

        /// <summary>
        /// Initializes a new instance of the <see cref="JTokenReader"/> class.
        /// </summary>
        /// <param name="token">The token to read from.</param>
        public JTokenReader(JToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            this._root = token;
        }

        // this is used by json.net schema
        internal JTokenReader(JToken token, string initialPath)
            : this(token)
        {
            this._initialPath = initialPath;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JToken"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            if (this.CurrentState != State.Start)
            {
                if (this._current == null)
                {
                    return false;
                }

                if (this._current is JContainer container && this._parent != container)
                {
                    return this.ReadInto(container);
                }
                else
                {
                    return this.ReadOver(this._current);
                }
            }

            this._current = this._root;
            this.SetToken(this._current);
            return true;
        }

        private bool ReadOver(JToken t)
        {
            if (t == this._root)
            {
                return this.ReadToEnd();
            }

            JToken next = t.Next;
            if ((next == null || next == t) || t == t.Parent.Last)
            {
                if (t.Parent == null)
                {
                    return this.ReadToEnd();
                }

                return this.SetEnd(t.Parent);
            }
            else
            {
                this._current = next;
                this.SetToken(this._current);
                return true;
            }
        }

        private bool ReadToEnd()
        {
            this._current = null;
            this.SetToken(JsonToken.None);
            return false;
        }

        private JsonToken? GetEndToken(JContainer c)
        {
            switch (c.Type)
            {
                case JTokenType.Object:
                    return JsonToken.EndObject;

                case JTokenType.Array:
                    return JsonToken.EndArray;

                case JTokenType.Constructor:
                    return JsonToken.EndConstructor;

                case JTokenType.Property:
                    return null;

                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(c.Type), c.Type, "Unexpected JContainer type.");
            }
        }

        private bool ReadInto(JContainer c)
        {
            JToken firstChild = c.First;
            if (firstChild == null)
            {
                return this.SetEnd(c);
            }
            else
            {
                this.SetToken(firstChild);
                this._current = firstChild;
                this._parent = c;
                return true;
            }
        }

        private bool SetEnd(JContainer c)
        {
            JsonToken? endToken = this.GetEndToken(c);
            if (endToken != null)
            {
                this.SetToken(endToken.GetValueOrDefault());
                this._current = c;
                this._parent = c;
                return true;
            }
            else
            {
                return this.ReadOver(c);
            }
        }

        private void SetToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    this.SetToken(JsonToken.StartObject);
                    break;

                case JTokenType.Array:
                    this.SetToken(JsonToken.StartArray);
                    break;

                case JTokenType.Constructor:
                    this.SetToken(JsonToken.StartConstructor, ((JConstructor)token).Name);
                    break;

                case JTokenType.Property:
                    this.SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
                    break;

                case JTokenType.Comment:
                    this.SetToken(JsonToken.Comment, ((JValue)token).Value);
                    break;

                case JTokenType.Integer:
                    this.SetToken(JsonToken.Integer, ((JValue)token).Value);
                    break;

                case JTokenType.Float:
                    this.SetToken(JsonToken.Float, ((JValue)token).Value);
                    break;

                case JTokenType.String:
                    this.SetToken(JsonToken.String, ((JValue)token).Value);
                    break;

                case JTokenType.Boolean:
                    this.SetToken(JsonToken.Boolean, ((JValue)token).Value);
                    break;

                case JTokenType.Null:
                    this.SetToken(JsonToken.Null, ((JValue)token).Value);
                    break;

                case JTokenType.Undefined:
                    this.SetToken(JsonToken.Undefined, ((JValue)token).Value);
                    break;

                case JTokenType.Date:
                    this.SetToken(JsonToken.Date, ((JValue)token).Value);
                    break;

                case JTokenType.Raw:
                    this.SetToken(JsonToken.Raw, ((JValue)token).Value);
                    break;

                case JTokenType.Bytes:
                    this.SetToken(JsonToken.Bytes, ((JValue)token).Value);
                    break;

                case JTokenType.Guid:
                    this.SetToken(JsonToken.String, this.SafeToString(((JValue)token).Value));
                    break;

                case JTokenType.Uri:
                    object v = ((JValue)token).Value;
                    Uri uri = v as Uri;
                    this.SetToken(JsonToken.String, uri != null ? uri.OriginalString : this.SafeToString(v));
                    break;

                case JTokenType.TimeSpan:
                    this.SetToken(JsonToken.String, this.SafeToString(((JValue)token).Value));
                    break;

                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token.Type), token.Type, "Unexpected JTokenType.");
            }
        }

        private string SafeToString(object value)
        {
            return value?.ToString();
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            if (this.CurrentState == State.Start)
            {
                return false;
            }

            IJsonLineInfo info = this._current;
            return (info != null && info.HasLineInfo());
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                if (this.CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo info = this._current;
                if (info != null)
                {
                    return info.LineNumber;
                }

                return 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                if (this.CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo info = this._current;
                if (info != null)
                {
                    return info.LinePosition;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the path of the current JSON token.
        /// </summary>
        public override string Path
        {
            get
            {
                string path = base.Path;

                if (this._initialPath == null)
                {
                    this._initialPath = this._root.Path;
                }

                if (!string.IsNullOrEmpty(this._initialPath))
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return this._initialPath;
                    }

                    if (path.StartsWith('['))
                    {
                        path = this._initialPath + path;
                    }
                    else
                    {
                        path = this._initialPath + "." + path;
                    }
                }

                return path;
            }
        }
    }
}