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
using System.Collections.ObjectModel;
using System.Globalization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    /// <summary>
    /// A collection of <see cref="JsonProperty"/> objects.
    /// </summary>
    public class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
    {
        private readonly Type _type;
        private readonly List<JsonProperty> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyCollection"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public JsonPropertyCollection(Type type)
            : base(StringComparer.Ordinal)
        {
            ValidationUtils.ArgumentNotNull(type, "type");
            this._type = type;

            // foreach over List<T> to avoid boxing the Enumerator
            this._list = (List<JsonProperty>)this.Items;
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(JsonProperty item)
        {
            return item.PropertyName;
        }

        /// <summary>
        /// Adds a <see cref="JsonProperty"/> object.
        /// </summary>
        /// <param name="property">The property to add to the collection.</param>
        public void AddProperty(JsonProperty property)
        {
            if (this.Contains(property.PropertyName))
            {
                // don't overwrite existing property with ignored property
                if (property.Ignored)
                {
                    return;
                }

                JsonProperty existingProperty = this[property.PropertyName];
                bool duplicateProperty = true;

                if (existingProperty.Ignored)
                {
                    // remove ignored property so it can be replaced in collection
                    this.Remove(existingProperty);
                    duplicateProperty = false;
                }
                else
                {
                    if (property.DeclaringType != null && existingProperty.DeclaringType != null)
                    {
                        if (property.DeclaringType.IsSubclassOf(existingProperty.DeclaringType)
                            || (existingProperty.DeclaringType.IsInterface() && property.DeclaringType.ImplementInterface(existingProperty.DeclaringType)))
                        {
                            // current property is on a derived class and hides the existing
                            this.Remove(existingProperty);
                            duplicateProperty = false;
                        }
                        if (existingProperty.DeclaringType.IsSubclassOf(property.DeclaringType)
                            || (property.DeclaringType.IsInterface() && existingProperty.DeclaringType.ImplementInterface(property.DeclaringType)))
                        {
                            // current property is hidden by the existing so don't add it
                            return;
                        }

                        if (this._type.ImplementInterface(existingProperty.DeclaringType) && this._type.ImplementInterface(property.DeclaringType))
                        {
                            // current property was already defined on another interface
                            return;
                        }
                    }
                }

                if (duplicateProperty)
                {
                    throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, this._type));
                }
            }

            this.Add(property);
        }

        /// <summary>
        /// Gets the closest matching <see cref="JsonProperty"/> object.
        /// First attempts to get an exact case match of <paramref name="propertyName"/> and then
        /// a case insensitive match.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>A matching property if found.</returns>
        public JsonProperty GetClosestMatchProperty(string propertyName)
        {
            JsonProperty property = this.GetProperty(propertyName, StringComparison.Ordinal);
            if (property == null)
            {
                property = this.GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);
            }

            return property;
        }

#if NETCOREAPP3_1_OR_GREATER
        private new bool TryGetValue(string key, out JsonProperty item)
        {
            if (this.Dictionary == null)
            {
                item = default(JsonProperty);
                return false;
            }

            return this.Dictionary.TryGetValue(key, out item);
        }
#else

        private bool TryGetValue(string key, out JsonProperty item)
        {
            if (this.Dictionary == null)
            {
                item = default(JsonProperty);
                return false;
            }

            return this.Dictionary.TryGetValue(key, out item);
        }

#endif

        /// <summary>
        /// Gets a property by property name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="comparisonType">Type property name string comparison.</param>
        /// <returns>A matching property if found.</returns>
        public JsonProperty GetProperty(string propertyName, StringComparison comparisonType)
        {
            // KeyedCollection has an ordinal comparer
            if (comparisonType == StringComparison.Ordinal)
            {
                if (this.TryGetValue(propertyName, out JsonProperty property))
                {
                    return property;
                }

                return null;
            }

            for (int i = 0; i < this._list.Count; i++)
            {
                JsonProperty property = this._list[i];
                if (string.Equals(propertyName, property.PropertyName, comparisonType))
                {
                    return property;
                }
            }

            return null;
        }
    }
}