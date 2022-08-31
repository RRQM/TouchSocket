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

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaNode
    {
        public string Id { get; }
        public ReadOnlyCollection<JsonSchema> Schemas { get; }
        public Dictionary<string, JsonSchemaNode> Properties { get; }
        public Dictionary<string, JsonSchemaNode> PatternProperties { get; }
        public List<JsonSchemaNode> Items { get; }
        public JsonSchemaNode AdditionalProperties { get; set; }
        public JsonSchemaNode AdditionalItems { get; set; }

        public JsonSchemaNode(JsonSchema schema)
        {
            this.Schemas = new ReadOnlyCollection<JsonSchema>(new[] { schema });
            this.Properties = new Dictionary<string, JsonSchemaNode>();
            this.PatternProperties = new Dictionary<string, JsonSchemaNode>();
            this.Items = new List<JsonSchemaNode>();

            this.Id = GetId(this.Schemas);
        }

        private JsonSchemaNode(JsonSchemaNode source, JsonSchema schema)
        {
            this.Schemas = new ReadOnlyCollection<JsonSchema>(source.Schemas.Union(new[] { schema }).ToList());
            this.Properties = new Dictionary<string, JsonSchemaNode>(source.Properties);
            this.PatternProperties = new Dictionary<string, JsonSchemaNode>(source.PatternProperties);
            this.Items = new List<JsonSchemaNode>(source.Items);
            this.AdditionalProperties = source.AdditionalProperties;
            this.AdditionalItems = source.AdditionalItems;

            this.Id = GetId(this.Schemas);
        }

        public JsonSchemaNode Combine(JsonSchema schema)
        {
            return new JsonSchemaNode(this, schema);
        }

        public static string GetId(IEnumerable<JsonSchema> schemata)
        {
            return string.Join("-", schemata.Select(s => s.InternalId).OrderBy(id => id, StringComparer.Ordinal)
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                    .ToArray()
#endif
                );
        }
    }
}