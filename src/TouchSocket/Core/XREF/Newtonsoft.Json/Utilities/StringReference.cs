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

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Utilities
{
    internal readonly struct StringReference
    {
        private readonly char[] _chars;
        private readonly int _startIndex;
        private readonly int _length;

        public char this[int i] => this._chars[i];

        public char[] Chars => this._chars;

        public int StartIndex => this._startIndex;

        public int Length => this._length;

        public StringReference(char[] chars, int startIndex, int length)
        {
            this._chars = chars;
            this._startIndex = startIndex;
            this._length = length;
        }

        public override string ToString()
        {
            return new string(this._chars, this._startIndex, this._length);
        }
    }

    internal static class StringReferenceExtensions
    {
        public static int IndexOf(this StringReference s, char c, int startIndex, int length)
        {
            int index = Array.IndexOf(s.Chars, c, s.StartIndex + startIndex, length);
            if (index == -1)
            {
                return -1;
            }

            return index - s.StartIndex;
        }

        public static bool StartsWith(this StringReference s, string text)
        {
            if (text.Length > s.Length)
            {
                return false;
            }

            char[] chars = s.Chars;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != chars[i + s.StartIndex])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EndsWith(this StringReference s, string text)
        {
            if (text.Length > s.Length)
            {
                return false;
            }

            char[] chars = s.Chars;

            int start = s.StartIndex + s.Length - text.Length;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != chars[i + start])
                {
                    return false;
                }
            }

            return true;
        }
    }
}