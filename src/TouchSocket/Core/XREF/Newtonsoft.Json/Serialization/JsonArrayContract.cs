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
using System.Collections.ObjectModel;
using System.Reflection;
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
    public class JsonArrayContract : JsonContainerContract
    {
        /// <summary>
        /// Gets the <see cref="System.Type"/> of the collection items.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the collection items.</value>
        public Type CollectionItemType { get; }

        /// <summary>
        /// Gets a value indicating whether the collection type is a multidimensional array.
        /// </summary>
        /// <value><c>true</c> if the collection type is a multidimensional array; otherwise, <c>false</c>.</value>
        public bool IsMultidimensionalArray { get; }

        private readonly Type _genericCollectionDefinitionType;

        private Type _genericWrapperType;
        private ObjectConstructor<object> _genericWrapperCreator;
        private Func<object> _genericTemporaryCollectionCreator;

        internal bool IsArray { get; }
        internal bool ShouldCreateWrapper { get; }
        internal bool CanDeserialize { get; private set; }

        private readonly ConstructorInfo _parameterizedConstructor;

        private ObjectConstructor<object> _parameterizedCreator;
        private ObjectConstructor<object> _overrideCreator;

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
            set
            {
                this._overrideCreator = value;
                // hacky
                this.CanDeserialize = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the creator has a parameter with the collection values.
        /// </summary>
        /// <value><c>true</c> if the creator has a parameter with the collection values; otherwise, <c>false</c>.</value>
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal => (this.HasParameterizedCreator || this._parameterizedCreator != null || this._parameterizedConstructor != null);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonArrayContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public JsonArrayContract(Type underlyingType)
            : base(underlyingType)
        {
            this.ContractType = JsonContractType.Array;
            this.IsArray = this.CreatedType.IsArray;

            bool canDeserialize;

            Type tempCollectionType;
            if (this.IsArray)
            {
                this.CollectionItemType = ReflectionUtils.GetCollectionItemType(this.UnderlyingType);
                this.IsReadOnlyOrFixedSize = true;
                this._genericCollectionDefinitionType = typeof(List<>).MakeGenericType(this.CollectionItemType);

                canDeserialize = true;
                this.IsMultidimensionalArray = (this.IsArray && this.UnderlyingType.GetArrayRank() > 1);
            }
            else if (typeof(IList).IsAssignableFrom(underlyingType))
            {
                if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out this._genericCollectionDefinitionType))
                {
                    this.CollectionItemType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
                }
                else
                {
                    this.CollectionItemType = ReflectionUtils.GetCollectionItemType(underlyingType);
                }

                if (underlyingType == typeof(IList))
                {
                    this.CreatedType = typeof(List<object>);
                }

                if (this.CollectionItemType != null)
                {
                    this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, this.CollectionItemType);
                }

                this.IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyCollection<>));
                canDeserialize = true;
            }
            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out this._genericCollectionDefinitionType))
            {
                this.CollectionItemType = this._genericCollectionDefinitionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(ICollection<>))
                    || ReflectionUtils.IsGenericDefinition(underlyingType, typeof(IList<>)))
                {
                    this.CreatedType = typeof(List<>).MakeGenericType(this.CollectionItemType);
                }

#if HAVE_ISET
                if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(ISet<>)))
                {
                    CreatedType = typeof(HashSet<>).MakeGenericType(CollectionItemType);
                }
#endif

                this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, this.CollectionItemType);
                canDeserialize = true;
                this.ShouldCreateWrapper = true;
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IReadOnlyCollection<>), out tempCollectionType))
            {
                CollectionItemType = tempCollectionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(IReadOnlyCollection<>))
                    || ReflectionUtils.IsGenericDefinition(underlyingType, typeof(IReadOnlyList<>)))
                {
                    CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(CollectionItemType);
                }

                _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(CreatedType, CollectionItemType);

#if HAVE_FSHARP_TYPES
                StoreFSharpListCreatorIfNecessary(underlyingType);
#endif

                IsReadOnlyOrFixedSize = true;
                canDeserialize = HasParameterizedCreatorInternal;
            }
#endif
            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IEnumerable<>), out tempCollectionType))
            {
                this.CollectionItemType = tempCollectionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(this.UnderlyingType, typeof(IEnumerable<>)))
                {
                    this.CreatedType = typeof(List<>).MakeGenericType(this.CollectionItemType);
                }

                this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, this.CollectionItemType);

#if HAVE_FSHARP_TYPES
                StoreFSharpListCreatorIfNecessary(underlyingType);
#endif

                if (underlyingType.IsGenericType() && underlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    this._genericCollectionDefinitionType = tempCollectionType;

                    this.IsReadOnlyOrFixedSize = false;
                    this.ShouldCreateWrapper = false;
                    canDeserialize = true;
                }
                else
                {
                    this._genericCollectionDefinitionType = typeof(List<>).MakeGenericType(this.CollectionItemType);

                    this.IsReadOnlyOrFixedSize = true;
                    this.ShouldCreateWrapper = true;
                    canDeserialize = this.HasParameterizedCreatorInternal;
                }
            }
            else
            {
                // types that implement IEnumerable and nothing else
                canDeserialize = false;
                this.ShouldCreateWrapper = true;
            }

            this.CanDeserialize = canDeserialize;

#if (NET20 || NET35)
            if (CollectionItemType != null && ReflectionUtils.IsNullableType(CollectionItemType))
            {
                // bug in .NET 2.0 & 3.5 that List<Nullable<T>> throws an error when adding null via IList.Add(object)
                // wrapper will handle calling Add(T) instead
                if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(List<>), out tempCollectionType)
                    || (IsArray && !IsMultidimensionalArray))
                {
                    ShouldCreateWrapper = true;
                }
            }
#endif

            if (ImmutableCollectionsUtils.TryBuildImmutableForArrayContract(
                underlyingType,
                this.CollectionItemType,
                out Type immutableCreatedType,
                out ObjectConstructor<object> immutableParameterizedCreator))
            {
                this.CreatedType = immutableCreatedType;
                this._parameterizedCreator = immutableParameterizedCreator;
                this.IsReadOnlyOrFixedSize = true;
                this.CanDeserialize = true;
            }
        }

        internal IWrappedCollection CreateWrapper(object list)
        {
            if (this._genericWrapperCreator == null)
            {
                this._genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(this.CollectionItemType);

                Type constructorArgument;

                if (ReflectionUtils.InheritsGenericDefinition(this._genericCollectionDefinitionType, typeof(List<>))
                    || this._genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    constructorArgument = typeof(ICollection<>).MakeGenericType(this.CollectionItemType);
                }
                else
                {
                    constructorArgument = this._genericCollectionDefinitionType;
                }

                ConstructorInfo genericWrapperConstructor = this._genericWrapperType.GetConstructor(new[] { constructorArgument });
                this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedCollection)this._genericWrapperCreator(list);
        }

        internal IList CreateTemporaryCollection()
        {
            if (this._genericTemporaryCollectionCreator == null)
            {
                // multidimensional array will also have array instances in it
                Type collectionItemType = (this.IsMultidimensionalArray || this.CollectionItemType == null)
                    ? typeof(object)
                    : this.CollectionItemType;

                Type temporaryListType = typeof(List<>).MakeGenericType(collectionItemType);
                this._genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryListType);
            }

            return (IList)this._genericTemporaryCollectionCreator();
        }

#if HAVE_FSHARP_TYPES
        private void StoreFSharpListCreatorIfNecessary(Type underlyingType)
        {
            if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpListTypeName)
            {
                FSharpUtils.EnsureInitialized(underlyingType.Assembly());
                _parameterizedCreator = FSharpUtils.CreateSeq(CollectionItemType);
            }
        }
#endif
    }
}