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

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal static class JsonSchemaConstants
    {
        public const string TypePropertyName = "type";
        public const string PropertiesPropertyName = "properties";
        public const string ItemsPropertyName = "items";
        public const string AdditionalItemsPropertyName = "additionalItems";
        public const string RequiredPropertyName = "required";
        public const string PatternPropertiesPropertyName = "patternProperties";
        public const string AdditionalPropertiesPropertyName = "additionalProperties";
        public const string RequiresPropertyName = "requires";
        public const string MinimumPropertyName = "minimum";
        public const string MaximumPropertyName = "maximum";
        public const string ExclusiveMinimumPropertyName = "exclusiveMinimum";
        public const string ExclusiveMaximumPropertyName = "exclusiveMaximum";
        public const string MinimumItemsPropertyName = "minItems";
        public const string MaximumItemsPropertyName = "maxItems";
        public const string PatternPropertyName = "pattern";
        public const string MaximumLengthPropertyName = "maxLength";
        public const string MinimumLengthPropertyName = "minLength";
        public const string EnumPropertyName = "enum";
        public const string ReadOnlyPropertyName = "readonly";
        public const string TitlePropertyName = "title";
        public const string DescriptionPropertyName = "description";
        public const string FormatPropertyName = "format";
        public const string DefaultPropertyName = "default";
        public const string TransientPropertyName = "transient";
        public const string DivisibleByPropertyName = "divisibleBy";
        public const string HiddenPropertyName = "hidden";
        public const string DisallowPropertyName = "disallow";
        public const string ExtendsPropertyName = "extends";
        public const string IdPropertyName = "id";
        public const string UniqueItemsPropertyName = "uniqueItems";

        public const string OptionValuePropertyName = "value";
        public const string OptionLabelPropertyName = "label";

        public static readonly IDictionary<string, JsonSchemaType> JsonSchemaTypeMapping = new Dictionary<string, JsonSchemaType>
        {
            { "string", JsonSchemaType.String },
            { "object", JsonSchemaType.Object },
            { "integer", JsonSchemaType.Integer },
            { "number", JsonSchemaType.Float },
            { "null", JsonSchemaType.Null },
            { "boolean", JsonSchemaType.Boolean },
            { "array", JsonSchemaType.Array },
            { "any", JsonSchemaType.Any }
        };
    }
}