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
using System.Text;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JsonPath
{
    internal class JPath
    {
        private static readonly char[] FloatCharacters = new[] { '.', 'E', 'e' };

        private readonly string _expression;
        public List<PathFilter> Filters { get; }

        private int _currentIndex;

        public JPath(string expression)
        {
            ValidationUtils.ArgumentNotNull(expression, nameof(expression));
            this._expression = expression;
            this.Filters = new List<PathFilter>();

            this.ParseMain();
        }

        private void ParseMain()
        {
            int currentPartStartIndex = this._currentIndex;

            this.EatWhitespace();

            if (this._expression.Length == this._currentIndex)
            {
                return;
            }

            if (this._expression[this._currentIndex] == '$')
            {
                if (this._expression.Length == 1)
                {
                    return;
                }

                // only increment position for "$." or "$["
                // otherwise assume property that starts with $
                char c = this._expression[this._currentIndex + 1];
                if (c == '.' || c == '[')
                {
                    this._currentIndex++;
                    currentPartStartIndex = this._currentIndex;
                }
            }

            if (!this.ParsePath(this.Filters, currentPartStartIndex, false))
            {
                int lastCharacterIndex = this._currentIndex;

                this.EatWhitespace();

                if (this._currentIndex < this._expression.Length)
                {
                    throw new JsonException("Unexpected character while parsing path: " + this._expression[lastCharacterIndex]);
                }
            }
        }

        private bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
        {
            bool scan = false;
            bool followingIndexer = false;
            bool followingDot = false;

            bool ended = false;
            while (this._currentIndex < this._expression.Length && !ended)
            {
                char currentChar = this._expression[this._currentIndex];

                switch (currentChar)
                {
                    case '[':
                    case '(':
                        if (this._currentIndex > currentPartStartIndex)
                        {
                            string member = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex);
                            if (member == "*")
                            {
                                member = null;
                            }

                            filters.Add(CreatePathFilter(member, scan));
                            scan = false;
                        }

                        filters.Add(this.ParseIndexer(currentChar, scan));
                        this._currentIndex++;
                        currentPartStartIndex = this._currentIndex;
                        followingIndexer = true;
                        followingDot = false;
                        break;

                    case ']':
                    case ')':
                        ended = true;
                        break;

                    case ' ':
                        if (this._currentIndex < this._expression.Length)
                        {
                            ended = true;
                        }
                        break;

                    case '.':
                        if (this._currentIndex > currentPartStartIndex)
                        {
                            string member = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex);
                            if (member == "*")
                            {
                                member = null;
                            }

                            filters.Add(CreatePathFilter(member, scan));
                            scan = false;
                        }
                        if (this._currentIndex + 1 < this._expression.Length && this._expression[this._currentIndex + 1] == '.')
                        {
                            scan = true;
                            this._currentIndex++;
                        }
                        this._currentIndex++;
                        currentPartStartIndex = this._currentIndex;
                        followingIndexer = false;
                        followingDot = true;
                        break;

                    default:
                        if (query && (currentChar == '=' || currentChar == '<' || currentChar == '!' || currentChar == '>' || currentChar == '|' || currentChar == '&'))
                        {
                            ended = true;
                        }
                        else
                        {
                            if (followingIndexer)
                            {
                                throw new JsonException("Unexpected character following indexer: " + currentChar);
                            }

                            this._currentIndex++;
                        }
                        break;
                }
            }

            bool atPathEnd = (this._currentIndex == this._expression.Length);

            if (this._currentIndex > currentPartStartIndex)
            {
                string member = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex).TrimEnd();
                if (member == "*")
                {
                    member = null;
                }
                filters.Add(CreatePathFilter(member, scan));
            }
            else
            {
                // no field name following dot in path and at end of base path/query
                if (followingDot && (atPathEnd || query))
                {
                    throw new JsonException("Unexpected end while parsing path.");
                }
            }

            return atPathEnd;
        }

        private static PathFilter CreatePathFilter(string member, bool scan)
        {
            if (scan)
            {
                return new ScanFilter { Name = member };
            }
            return new FieldFilter { Name = member };
        }

        private PathFilter ParseIndexer(char indexerOpenChar, bool scan)
        {
            this._currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            this.EnsureLength("Path ended with open indexer.");

            this.EatWhitespace();

            if (this._expression[this._currentIndex] == '\'')
            {
                return this.ParseQuotedField(indexerCloseChar, scan);
            }
            else if (this._expression[this._currentIndex] == '?')
            {
                return this.ParseQuery(indexerCloseChar, scan);
            }
            else
            {
                return this.ParseArrayIndexer(indexerCloseChar);
            }
        }

        private PathFilter ParseArrayIndexer(char indexerCloseChar)
        {
            int start = this._currentIndex;
            int? end = null;
            List<int> indexes = null;
            int colonCount = 0;
            int? startIndex = null;
            int? endIndex = null;
            int? step = null;

            while (this._currentIndex < this._expression.Length)
            {
                char currentCharacter = this._expression[this._currentIndex];

                if (currentCharacter == ' ')
                {
                    end = this._currentIndex;
                    this.EatWhitespace();
                    continue;
                }

                if (currentCharacter == indexerCloseChar)
                {
                    int length = (end ?? this._currentIndex) - start;

                    if (indexes != null)
                    {
                        if (length == 0)
                        {
                            throw new JsonException("Array index expected.");
                        }

                        string indexer = this._expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        indexes.Add(index);
                        return new ArrayMultipleIndexFilter { Indexes = indexes };
                    }
                    else if (colonCount > 0)
                    {
                        if (length > 0)
                        {
                            string indexer = this._expression.Substring(start, length);
                            int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                            if (colonCount == 1)
                            {
                                endIndex = index;
                            }
                            else
                            {
                                step = index;
                            }
                        }

                        return new ArraySliceFilter { Start = startIndex, End = endIndex, Step = step };
                    }
                    else
                    {
                        if (length == 0)
                        {
                            throw new JsonException("Array index expected.");
                        }

                        string indexer = this._expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        return new ArrayIndexFilter { Index = index };
                    }
                }
                else if (currentCharacter == ',')
                {
                    int length = (end ?? this._currentIndex) - start;

                    if (length == 0)
                    {
                        throw new JsonException("Array index expected.");
                    }

                    if (indexes == null)
                    {
                        indexes = new List<int>();
                    }

                    string indexer = this._expression.Substring(start, length);
                    indexes.Add(Convert.ToInt32(indexer, CultureInfo.InvariantCulture));

                    this._currentIndex++;

                    this.EatWhitespace();

                    start = this._currentIndex;
                    end = null;
                }
                else if (currentCharacter == '*')
                {
                    this._currentIndex++;
                    this.EnsureLength("Path ended with open indexer.");
                    this.EatWhitespace();

                    if (this._expression[this._currentIndex] != indexerCloseChar)
                    {
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                    }

                    return new ArrayIndexFilter();
                }
                else if (currentCharacter == ':')
                {
                    int length = (end ?? this._currentIndex) - start;

                    if (length > 0)
                    {
                        string indexer = this._expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        if (colonCount == 0)
                        {
                            startIndex = index;
                        }
                        else if (colonCount == 1)
                        {
                            endIndex = index;
                        }
                        else
                        {
                            step = index;
                        }
                    }

                    colonCount++;

                    this._currentIndex++;

                    this.EatWhitespace();

                    start = this._currentIndex;
                    end = null;
                }
                else if (!char.IsDigit(currentCharacter) && currentCharacter != '-')
                {
                    throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                }
                else
                {
                    if (end != null)
                    {
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                    }

                    this._currentIndex++;
                }
            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void EatWhitespace()
        {
            while (this._currentIndex < this._expression.Length)
            {
                if (this._expression[this._currentIndex] != ' ')
                {
                    break;
                }

                this._currentIndex++;
            }
        }

        private PathFilter ParseQuery(char indexerCloseChar, bool scan)
        {
            this._currentIndex++;
            this.EnsureLength("Path ended with open indexer.");

            if (this._expression[this._currentIndex] != '(')
            {
                throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
            }

            this._currentIndex++;

            QueryExpression expression = this.ParseExpression();

            this._currentIndex++;
            this.EnsureLength("Path ended with open indexer.");
            this.EatWhitespace();

            if (this._expression[this._currentIndex] != indexerCloseChar)
            {
                throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
            }

            if (!scan)
            {
                return new QueryFilter
                {
                    Expression = expression
                };
            }
            else
            {
                return new QueryScanFilter
                {
                    Expression = expression
                };
            }
        }

        private bool TryParseExpression(out List<PathFilter> expressionPath)
        {
            if (this._expression[this._currentIndex] == '$')
            {
                expressionPath = new List<PathFilter>();
                expressionPath.Add(RootFilter.Instance);
            }
            else if (this._expression[this._currentIndex] == '@')
            {
                expressionPath = new List<PathFilter>();
            }
            else
            {
                expressionPath = null;
                return false;
            }

            this._currentIndex++;

            if (this.ParsePath(expressionPath, this._currentIndex, true))
            {
                throw new JsonException("Path ended with open query.");
            }

            return true;
        }

        private JsonException CreateUnexpectedCharacterException()
        {
            return new JsonException("Unexpected character while parsing path query: " + this._expression[this._currentIndex]);
        }

        private object ParseSide()
        {
            this.EatWhitespace();

            if (this.TryParseExpression(out var expressionPath))
            {
                this.EatWhitespace();
                this.EnsureLength("Path ended with open query.");

                return expressionPath;
            }

            if (this.TryParseValue(out var value))
            {
                this.EatWhitespace();
                this.EnsureLength("Path ended with open query.");

                return new JValue(value);
            }

            throw this.CreateUnexpectedCharacterException();
        }

        private QueryExpression ParseExpression()
        {
            QueryExpression rootExpression = null;
            CompositeExpression parentExpression = null;

            while (this._currentIndex < this._expression.Length)
            {
                object left = this.ParseSide();
                object right = null;

                QueryOperator op;
                if (this._expression[this._currentIndex] == ')'
                    || this._expression[this._currentIndex] == '|'
                    || this._expression[this._currentIndex] == '&')
                {
                    op = QueryOperator.Exists;
                }
                else
                {
                    op = this.ParseOperator();

                    right = this.ParseSide();
                }

                BooleanQueryExpression booleanExpression = new BooleanQueryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
                };

                if (this._expression[this._currentIndex] == ')')
                {
                    if (parentExpression != null)
                    {
                        parentExpression.Expressions.Add(booleanExpression);
                        return rootExpression;
                    }

                    return booleanExpression;
                }
                if (this._expression[this._currentIndex] == '&')
                {
                    if (!this.Match("&&"))
                    {
                        throw this.CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.And)
                    {
                        CompositeExpression andExpression = new CompositeExpression { Operator = QueryOperator.And };

                        parentExpression?.Expressions.Add(andExpression);

                        parentExpression = andExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
                if (this._expression[this._currentIndex] == '|')
                {
                    if (!this.Match("||"))
                    {
                        throw this.CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.Or)
                    {
                        CompositeExpression orExpression = new CompositeExpression { Operator = QueryOperator.Or };

                        parentExpression?.Expressions.Add(orExpression);

                        parentExpression = orExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
            }

            throw new JsonException("Path ended with open query.");
        }

        private bool TryParseValue(out object value)
        {
            char currentChar = this._expression[this._currentIndex];
            if (currentChar == '\'')
            {
                value = this.ReadQuotedString();
                return true;
            }
            else if (char.IsDigit(currentChar) || currentChar == '-')
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(currentChar);

                this._currentIndex++;
                while (this._currentIndex < this._expression.Length)
                {
                    currentChar = this._expression[this._currentIndex];
                    if (currentChar == ' ' || currentChar == ')')
                    {
                        string numberText = sb.ToString();

                        if (numberText.IndexOfAny(FloatCharacters) != -1)
                        {
                            bool result = double.TryParse(numberText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d);
                            value = d;
                            return result;
                        }
                        else
                        {
                            bool result = long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l);
                            value = l;
                            return result;
                        }
                    }
                    else
                    {
                        sb.Append(currentChar);
                        this._currentIndex++;
                    }
                }
            }
            else if (currentChar == 't')
            {
                if (this.Match("true"))
                {
                    value = true;
                    return true;
                }
            }
            else if (currentChar == 'f')
            {
                if (this.Match("false"))
                {
                    value = false;
                    return true;
                }
            }
            else if (currentChar == 'n')
            {
                if (this.Match("null"))
                {
                    value = null;
                    return true;
                }
            }
            else if (currentChar == '/')
            {
                value = this.ReadRegexString();
                return true;
            }

            value = null;
            return false;
        }

        private string ReadQuotedString()
        {
            StringBuilder sb = new StringBuilder();

            this._currentIndex++;
            while (this._currentIndex < this._expression.Length)
            {
                char currentChar = this._expression[this._currentIndex];
                if (currentChar == '\\' && this._currentIndex + 1 < this._expression.Length)
                {
                    this._currentIndex++;
                    currentChar = this._expression[this._currentIndex];

                    char resolvedChar;
                    switch (currentChar)
                    {
                        case 'b':
                            resolvedChar = '\b';
                            break;

                        case 't':
                            resolvedChar = '\t';
                            break;

                        case 'n':
                            resolvedChar = '\n';
                            break;

                        case 'f':
                            resolvedChar = '\f';
                            break;

                        case 'r':
                            resolvedChar = '\r';
                            break;

                        case '\\':
                        case '"':
                        case '\'':
                        case '/':
                            resolvedChar = currentChar;
                            break;

                        default:
                            throw new JsonException(@"Unknown escape character: \" + currentChar);
                    }

                    sb.Append(resolvedChar);

                    this._currentIndex++;
                }
                else if (currentChar == '\'')
                {
                    this._currentIndex++;
                    return sb.ToString();
                }
                else
                {
                    this._currentIndex++;
                    sb.Append(currentChar);
                }
            }

            throw new JsonException("Path ended with an open string.");
        }

        private string ReadRegexString()
        {
            int startIndex = this._currentIndex;

            this._currentIndex++;
            while (this._currentIndex < this._expression.Length)
            {
                char currentChar = this._expression[this._currentIndex];

                // handle escaped / character
                if (currentChar == '\\' && this._currentIndex + 1 < this._expression.Length)
                {
                    this._currentIndex += 2;
                }
                else if (currentChar == '/')
                {
                    this._currentIndex++;

                    while (this._currentIndex < this._expression.Length)
                    {
                        currentChar = this._expression[this._currentIndex];

                        if (char.IsLetter(currentChar))
                        {
                            this._currentIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return this._expression.Substring(startIndex, this._currentIndex - startIndex);
                }
                else
                {
                    this._currentIndex++;
                }
            }

            throw new JsonException("Path ended with an open regex.");
        }

        private bool Match(string s)
        {
            int currentPosition = this._currentIndex;
            foreach (char c in s)
            {
                if (currentPosition < this._expression.Length && this._expression[currentPosition] == c)
                {
                    currentPosition++;
                }
                else
                {
                    return false;
                }
            }

            this._currentIndex = currentPosition;
            return true;
        }

        private QueryOperator ParseOperator()
        {
            if (this._currentIndex + 1 >= this._expression.Length)
            {
                throw new JsonException("Path ended with open query.");
            }

            if (this.Match("=="))
            {
                return QueryOperator.Equals;
            }

            if (this.Match("=~"))
            {
                return QueryOperator.RegexEquals;
            }

            if (this.Match("!=") || this.Match("<>"))
            {
                return QueryOperator.NotEquals;
            }
            if (this.Match("<="))
            {
                return QueryOperator.LessThanOrEquals;
            }
            if (this.Match("<"))
            {
                return QueryOperator.LessThan;
            }
            if (this.Match(">="))
            {
                return QueryOperator.GreaterThanOrEquals;
            }
            if (this.Match(">"))
            {
                return QueryOperator.GreaterThan;
            }

            throw new JsonException("Could not read query operator.");
        }

        private PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
        {
            List<string> fields = null;

            while (this._currentIndex < this._expression.Length)
            {
                string field = this.ReadQuotedString();

                this.EatWhitespace();
                this.EnsureLength("Path ended with open indexer.");

                if (this._expression[this._currentIndex] == indexerCloseChar)
                {
                    if (fields != null)
                    {
                        fields.Add(field);
                        if (scan)
                        {
                            return new ScanMultipleFilter { Names = fields };
                        }
                        else
                        {
                            return new FieldMultipleFilter { Names = fields };
                        }
                    }
                    else
                    {
                        return CreatePathFilter(field, scan);
                    }
                }
                else if (this._expression[this._currentIndex] == ',')
                {
                    this._currentIndex++;
                    this.EatWhitespace();

                    if (fields == null)
                    {
                        fields = new List<string>();
                    }

                    fields.Add(field);
                }
                else
                {
                    throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
                }
            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void EnsureLength(string message)
        {
            if (this._currentIndex >= this._expression.Length)
            {
                throw new JsonException(message);
            }
        }

        internal IEnumerable<JToken> Evaluate(JToken root, JToken t, bool errorWhenNoMatch)
        {
            return Evaluate(this.Filters, root, t, errorWhenNoMatch);
        }

        internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, bool errorWhenNoMatch)
        {
            IEnumerable<JToken> current = new[] { t };
            foreach (PathFilter filter in filters)
            {
                current = filter.ExecuteFilter(root, current, errorWhenNoMatch);
            }

            return current;
        }
    }
}