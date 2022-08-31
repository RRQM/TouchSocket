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
    /// <summary>
    /// Builds a string. Unlike <see cref="System.Text.StringBuilder"/> this class lets you reuse its internal buffer.
    /// </summary>
    internal struct StringBuffer
    {
        private char[] _buffer;
        private int _position;

        public int Position
        {
            get => this._position;
            set => this._position = value;
        }

        public bool IsEmpty => this._buffer == null;

        public StringBuffer(IArrayPool<char> bufferPool, int initalSize) : this(BufferUtils.RentBuffer(bufferPool, initalSize))
        {
        }

        private StringBuffer(char[] buffer)
        {
            this._buffer = buffer;
            this._position = 0;
        }

        public void Append(IArrayPool<char> bufferPool, char value)
        {
            // test if the buffer array is large enough to take the value
            if (this._position == this._buffer.Length)
            {
                this.EnsureSize(bufferPool, 1);
            }

            // set value and increment poisition
            this._buffer[this._position++] = value;
        }

        public void Append(IArrayPool<char> bufferPool, char[] buffer, int startIndex, int count)
        {
            if (this._position + count >= this._buffer.Length)
            {
                this.EnsureSize(bufferPool, count);
            }

            Array.Copy(buffer, startIndex, this._buffer, this._position, count);

            this._position += count;
        }

        public void Clear(IArrayPool<char> bufferPool)
        {
            if (this._buffer != null)
            {
                BufferUtils.ReturnBuffer(bufferPool, this._buffer);
                this._buffer = null;
            }
            this._position = 0;
        }

        private void EnsureSize(IArrayPool<char> bufferPool, int appendLength)
        {
            char[] newBuffer = BufferUtils.RentBuffer(bufferPool, (this._position + appendLength) * 2);

            if (this._buffer != null)
            {
                Array.Copy(this._buffer, newBuffer, this._position);
                BufferUtils.ReturnBuffer(bufferPool, this._buffer);
            }

            this._buffer = newBuffer;
        }

        public override string ToString()
        {
            return this.ToString(0, this._position);
        }

        public string ToString(int start, int length)
        {
            // TODO: validation
            return new string(this._buffer, start, length);
        }

        public char[] InternalBuffer => this._buffer;
    }
}