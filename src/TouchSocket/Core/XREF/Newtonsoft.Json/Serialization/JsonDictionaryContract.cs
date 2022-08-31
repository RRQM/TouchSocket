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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

#if !HAVE_LINQ
#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class JsonDictionaryContract : JsonContainerContract
    {
        /// <summary>
        /// Gets or sets the dictionary key resolver.
        /// </summary>
        /// <value>The dictionary key resolver.</value>
        public Func<string, string> DictionaryKeyResolver { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary keys.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary keys.</value>
        public Type DictionaryKeyType { get; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary values.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary values.</value>
        public Type DictionaryValueType { get; }

        internal JsonContract KeyContract { get; set; }

        private readonly Type _genericCollectionDefinitionType;

        private Type _genericWrapperType;
        private ObjectConstructor<object> _genericWrapperCreator;

        private Func<object> _genericTemporaryDictionaryCreator;

        internal bool ShouldCreateWrapper { get; }

        private readonly ConstructorInfo _parameterizedConstructor;

        private ObjectConstructor<object> _overrideCreator;
        private ObjectConstructor<object> _parameterizedCreator;

        internal ObjectConstructor<object> ParameterizedCreator
        {
            get
            {
                if (this._parameterizedCreator == null)
                {
                    this._parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(this._parameterizedConstructor);
                }

                return this._parameterizedCreator;
            }
        }

        /// <summary>
        /// Gets or sets the function used to create the object. When set this function will override <see cref="JsonContract.DefaultCreator"/>.
        /// </summary>
        /// <value>The function used to create the object.</value>
        public ObjectConstructor<object> OverrideCreator
        {
            get => this._overrideCreator;
            set => this._overrideCreator = value;
        }

        /// <summary>
        /// Gets a value indicating whether the creator has a parameter with the dictionary values.
        /// </summary>
        /// <value><c>true</c> if the creator has a parameter with the dictionary values; otherwise, <c>false</c>.</value>
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal => (this.HasParameterizedCreator || this._parameterizedCreator != null || this._parameterizedConstructor != null);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDictionaryContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public JsonDictionaryContract(Type underlyingType)
            : base(underlyingType)
        {
            this.ContractType = JsonContractType.Dictionary;

            Type keyType;
            Type valueType;

            if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<,>), out this._genericCollectionDefinitionType))
            {
                keyType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = this._genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(this.UnderlyingType, typeof(IDictionary<,>)))
                {
                    this.CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                }
                else if (underlyingType.IsGenericType())
                {
                    // ConcurrentDictionary<,> + IDictionary setter + null value = error
                    // wrap to use generic setter
                    // https://github.com/JamesNK/TouchSocket.Core.XREF.Newtonsoft.Json/issues/1582
                    Type typeDefinition = underlyingType.GetGenericTypeDefinition();
                    if (typeDefinition.FullName == JsonTypeReflector.ConcurrentDictionaryTypeName)
                    {
                        this.ShouldCreateWrapper = true;
                    }
                }

#if HAVE_READ_ONLY_COLLECTIONS
                IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyDictionary<,>));
#endif
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IReadOnlyDictionary<,>), out _genericCollectionDefinitionType))
            {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IReadOnlyDictionary<,>)))
                {
                    CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
                }

                IsReadOnlyOrFixedSize = true;
            }
#endif
            else
            {
                ReflectionUtils.GetDictionaryKeyValueTypes(this.UnderlyingType, out keyType, out valueType);

                if (this.UnderlyingType == typeof(IDictionary))
                {
                    this.CreatedType = typeof(Dictionary<object, object>);
                }
            }

            if (keyType != null && valueType != null)
            {
                this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(
                    this.CreatedType,
                    typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType),
                    typeof(IDictionary<,>).MakeGenericType(keyType, valueType));

#if HAVE_FSHARP_TYPES
                if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpMapTypeName)
                {
                    FSharpUtils.EnsureInitialized(underlyingType.Assembly());
                    _parameterizedCreator = FSharpUtils.CreateMap(keyType, valueType);
                }
#endif
            }

            if (!typeof(IDictionary).IsAssignableFrom(this.CreatedType))
            {
                this.ShouldCreateWrapper = true;
            }

            this.DictionaryKeyType = keyType;
            this.DictionaryValueType = valueType;

#if (NET20 || NET35)
            if (DictionaryValueType != null && ReflectionUtils.IsNullableType(DictionaryValueType))
            {
                // bug in .NET 2.0 & 3.5 that Dictionary<TKey, Nullable<TValue>> throws an error when adding null via IDictionary[key] = object
                // wrapper will handle calling Add(T) instead
                if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(Dictionary<,>), out _))
                {
                    ShouldCreateWrapper = true;
                }
            }
#endif

            if (ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(
                underlyingType,
                this.DictionaryKeyType,
                this.DictionaryValueType,
                out Type immutableCreatedType,
                out ObjectConstructor<object> immutableParameterizedCreator))
            {
                this.CreatedType = immutableCreatedType;
                this._parameterizedCreator = immutableParameterizedCreator;
                this.IsReadOnlyOrFixedSize = true;
            }
        }

        internal IWrappedDictionary CreateWrapper(object dictionary)
        {
            if (this._genericWrapperCreator == null)
            {
                this._genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(this.DictionaryKeyType, this.DictionaryValueType);

                ConstructorInfo genericWrapperConstructor = this._genericWrapperType.GetConstructor(new[] { this._genericCollectionDefinitionType });
                this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedDictionary)this._genericWrapperCreator(dictionary);
        }

        internal IDictionary CreateTemporaryDictionary()
        {
            if (this._genericTemporaryDictionaryCreator == null)
            {
                Type temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(this.DictionaryKeyType ?? typeof(object), this.DictionaryValueType ?? typeof(object));

                this._genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
            }

            return (IDictionary)this._genericTemporaryDictionaryCreator();
        }
    }
}