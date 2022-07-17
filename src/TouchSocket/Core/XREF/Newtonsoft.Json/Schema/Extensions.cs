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
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Schema
{
    /// <summary>
    /// <para>
    /// Contains the JSON schema extension methods.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public static class Extensions
    {
        /// <summary>
        /// <para>
        /// Determines whether the <see cref="JToken"/> is valid.
        /// </para>
        /// <note type="caution">
        /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
        /// </note>
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="JToken"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
        public static bool IsValid(this JToken source, JsonSchema schema)
        {
            bool valid = true;
            source.Validate(schema, (sender, args) => { valid = false; });
            return valid;
        }

        /// <summary>
        /// <para>
        /// Determines whether the <see cref="JToken"/> is valid.
        /// </para>
        /// <note type="caution">
        /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
        /// </note>
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <param name="errorMessages">When this method returns, contains any error messages generated while validating. </param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="JToken"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
        public static bool IsValid(this JToken source, JsonSchema schema, out IList<string> errorMessages)
        {
            IList<string> errors = new List<string>();

            source.Validate(schema, (sender, args) => errors.Add(args.Message));

            errorMessages = errors;
            return (errorMessages.Count == 0);
        }

        /// <summary>
        /// <para>
        /// Validates the specified <see cref="JToken"/>.
        /// </para>
        /// <note type="caution">
        /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
        /// </note>
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
        public static void Validate(this JToken source, JsonSchema schema)
        {
            source.Validate(schema, null);
        }

        /// <summary>
        /// <para>
        /// Validates the specified <see cref="JToken"/>.
        /// </para>
        /// <note type="caution">
        /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
        /// </note>
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <param name="validationEventHandler">The validation event handler.</param>
        [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
        public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));
            ValidationUtils.ArgumentNotNull(schema, nameof(schema));

            using (JsonValidatingReader reader = new JsonValidatingReader(source.CreateReader()))
            {
                reader.Schema = schema;
                if (validationEventHandler != null)
                {
                    reader.ValidationEventHandler += validationEventHandler;
                }

                while (reader.Read())
                {
                }
            }
        }
    }
}