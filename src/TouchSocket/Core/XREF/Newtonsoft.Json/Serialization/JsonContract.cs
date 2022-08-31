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
using System.Reflection;
using System.Runtime.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal enum JsonContractType
    {
        None = 0,
        Object = 1,
        Array = 2,
        Primitive = 3,
        String = 4,
        Dictionary = 5,
        Dynamic = 6,
        Serializable = 7,
        Linq = 8
    }

    /// <summary>
    /// Handles <see cref="JsonSerializer"/> serialization callback events.
    /// </summary>
    /// <param name="o">The object that raised the callback event.</param>
    /// <param name="context">The streaming context.</param>
    public delegate void SerializationCallback(object o, StreamingContext context);

    /// <summary>
    /// Handles <see cref="JsonSerializer"/> serialization error callback events.
    /// </summary>
    /// <param name="o">The object that raised the callback event.</param>
    /// <param name="context">The streaming context.</param>
    /// <param name="errorContext">The error context.</param>
    public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);

    /// <summary>
    /// Sets extension data for an object during deserialization.
    /// </summary>
    /// <param name="o">The object to set extension data on.</param>
    /// <param name="key">The extension data key.</param>
    /// <param name="value">The extension data value.</param>
    public delegate void ExtensionDataSetter(object o, string key, object value);

    /// <summary>
    /// Gets extension data for an object during serialization.
    /// </summary>
    /// <param name="o">The object to set extension data on.</param>
    public delegate IEnumerable<KeyValuePair<object, object>> ExtensionDataGetter(object o);

    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public abstract class JsonContract
    {
        internal bool IsNullable;
        internal bool IsConvertable;
        internal bool IsEnum;
        internal Type NonNullableUnderlyingType;
        internal ReadType InternalReadType;
        internal JsonContractType ContractType;
        internal bool IsReadOnlyOrFixedSize;
        internal bool IsSealed;
        internal bool IsInstantiable;

        private List<SerializationCallback> _onDeserializedCallbacks;
        private IList<SerializationCallback> _onDeserializingCallbacks;
        private IList<SerializationCallback> _onSerializedCallbacks;
        private IList<SerializationCallback> _onSerializingCallbacks;
        private IList<SerializationErrorCallback> _onErrorCallbacks;
        private Type _createdType;

        /// <summary>
        /// Gets the underlying type for the contract.
        /// </summary>
        /// <value>The underlying type for the contract.</value>
        public Type UnderlyingType { get; }

        /// <summary>
        /// Gets or sets the type created during deserialization.
        /// </summary>
        /// <value>The type created during deserialization.</value>
        public Type CreatedType
        {
            get => this._createdType;
            set
            {
                this._createdType = value;

                this.IsSealed = this._createdType.IsSealed();
                this.IsInstantiable = !(this._createdType.IsInterface() || this._createdType.IsAbstract());
            }
        }

        /// <summary>
        /// Gets or sets whether this type contract is serialized as a reference.
        /// </summary>
        /// <value>Whether this type contract is serialized as a reference.</value>
        public bool? IsReference { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="JsonConverter" /> for this contract.
        /// </summary>
        /// <value>The converter.</value>
        public JsonConverter Converter { get; set; }

        // internally specified JsonConverter's to override default behavour
        // checked for after passed in converters and attribute specified converters
        internal JsonConverter InternalConverter { get; set; }

        /// <summary>
        /// Gets or sets all methods called immediately after deserialization of the object.
        /// </summary>
        /// <value>The methods called immediately after deserialization of the object.</value>
        public IList<SerializationCallback> OnDeserializedCallbacks
        {
            get
            {
                if (this._onDeserializedCallbacks == null)
                {
                    this._onDeserializedCallbacks = new List<SerializationCallback>();
                }

                return this._onDeserializedCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called during deserialization of the object.
        /// </summary>
        /// <value>The methods called during deserialization of the object.</value>
        public IList<SerializationCallback> OnDeserializingCallbacks
        {
            get
            {
                if (this._onDeserializingCallbacks == null)
                {
                    this._onDeserializingCallbacks = new List<SerializationCallback>();
                }

                return this._onDeserializingCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called after serialization of the object graph.
        /// </summary>
        /// <value>The methods called after serialization of the object graph.</value>
        public IList<SerializationCallback> OnSerializedCallbacks
        {
            get
            {
                if (this._onSerializedCallbacks == null)
                {
                    this._onSerializedCallbacks = new List<SerializationCallback>();
                }

                return this._onSerializedCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called before serialization of the object.
        /// </summary>
        /// <value>The methods called before serialization of the object.</value>
        public IList<SerializationCallback> OnSerializingCallbacks
        {
            get
            {
                if (this._onSerializingCallbacks == null)
                {
                    this._onSerializingCallbacks = new List<SerializationCallback>();
                }

                return this._onSerializingCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all method called when an error is thrown during the serialization of the object.
        /// </summary>
        /// <value>The methods called when an error is thrown during the serialization of the object.</value>
        public IList<SerializationErrorCallback> OnErrorCallbacks
        {
            get
            {
                if (this._onErrorCallbacks == null)
                {
                    this._onErrorCallbacks = new List<SerializationErrorCallback>();
                }

                return this._onErrorCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets the default creator method used to create the object.
        /// </summary>
        /// <value>The default creator method used to create the object.</value>
        public Func<object> DefaultCreator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default creator is non-public.
        /// </summary>
        /// <value><c>true</c> if the default object creator is non-public; otherwise, <c>false</c>.</value>
        public bool DefaultCreatorNonPublic { get; set; }

        internal JsonContract(Type underlyingType)
        {
            ValidationUtils.ArgumentNotNull(underlyingType, nameof(underlyingType));

            this.UnderlyingType = underlyingType;

            this.IsNullable = ReflectionUtils.IsNullable(underlyingType);
            this.NonNullableUnderlyingType = (this.IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType;

            this.CreatedType = this.NonNullableUnderlyingType;

            this.IsConvertable = ConvertUtils.IsConvertible(this.NonNullableUnderlyingType);
            this.IsEnum = this.NonNullableUnderlyingType.IsEnum();

            this.InternalReadType = ReadType.Read;
        }

        internal void InvokeOnSerializing(object o, StreamingContext context)
        {
            if (this._onSerializingCallbacks != null)
            {
                foreach (SerializationCallback callback in this._onSerializingCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnSerialized(object o, StreamingContext context)
        {
            if (this._onSerializedCallbacks != null)
            {
                foreach (SerializationCallback callback in this._onSerializedCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserializing(object o, StreamingContext context)
        {
            if (this._onDeserializingCallbacks != null)
            {
                foreach (SerializationCallback callback in this._onDeserializingCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserialized(object o, StreamingContext context)
        {
            if (this._onDeserializedCallbacks != null)
            {
                foreach (SerializationCallback callback in this._onDeserializedCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
        {
            if (this._onErrorCallbacks != null)
            {
                foreach (SerializationErrorCallback callback in this._onErrorCallbacks)
                {
                    callback(o, context, errorContext);
                }
            }
        }

        internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context) => callbackMethodInfo.Invoke(o, new object[] { context });
        }

        internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] { context, econtext });
        }
    }
}