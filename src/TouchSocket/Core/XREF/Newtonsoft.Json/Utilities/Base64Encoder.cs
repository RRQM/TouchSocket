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
using System.IO;

#if HAVE_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Utilities
{
    internal class Base64Encoder
    {
        private const int Base64LineSize = 76;
        private const int LineSizeInBytes = 57;

        private readonly char[] _charsLine = new char[Base64LineSize];
        private readonly TextWriter _writer;

        private byte[] _leftOverBytes;
        private int _leftOverBytesCount;

        public Base64Encoder(TextWriter writer)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            this._writer = writer;
        }

        private void ValidateEncode(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > (buffer.Length - index))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        public void Encode(byte[] buffer, int index, int count)
        {
            this.ValidateEncode(buffer, index, count);

            if (this._leftOverBytesCount > 0)
            {
                if (this.FulfillFromLeftover(buffer, index, ref count))
                {
                    return;
                }

                int num2 = Convert.ToBase64CharArray(this._leftOverBytes, 0, 3, this._charsLine, 0);
                this.WriteChars(this._charsLine, 0, num2);
            }

            this.StoreLeftOverBytes(buffer, index, ref count);

            int num4 = index + count;
            int length = LineSizeInBytes;
            while (index < num4)
            {
                if ((index + length) > num4)
                {
                    length = num4 - index;
                }
                int num6 = Convert.ToBase64CharArray(buffer, index, length, this._charsLine, 0);
                this.WriteChars(this._charsLine, 0, num6);
                index += length;
            }
        }

        private void StoreLeftOverBytes(byte[] buffer, int index, ref int count)
        {
            int leftOverBytesCount = count % 3;
            if (leftOverBytesCount > 0)
            {
                count -= leftOverBytesCount;
                if (this._leftOverBytes == null)
                {
                    this._leftOverBytes = new byte[3];
                }

                for (int i = 0; i < leftOverBytesCount; i++)
                {
                    this._leftOverBytes[i] = buffer[index + count + i];
                }
            }

            this._leftOverBytesCount = leftOverBytesCount;
        }

        private bool FulfillFromLeftover(byte[] buffer, int index, ref int count)
        {
            int leftOverBytesCount = this._leftOverBytesCount;
            while (leftOverBytesCount < 3 && count > 0)
            {
                this._leftOverBytes[leftOverBytesCount++] = buffer[index++];
                count--;
            }

            if (count == 0 && leftOverBytesCount < 3)
            {
                this._leftOverBytesCount = leftOverBytesCount;
                return true;
            }

            return false;
        }

        public void Flush()
        {
            if (this._leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(this._leftOverBytes, 0, this._leftOverBytesCount, this._charsLine, 0);
                this.WriteChars(this._charsLine, 0, count);
                this._leftOverBytesCount = 0;
            }
        }

        private void WriteChars(char[] chars, int index, int count)
        {
            this._writer.Write(chars, index, count);
        }

#if HAVE_ASYNC

        public async Task EncodeAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            ValidateEncode(buffer, index, count);

            if (_leftOverBytesCount > 0)
            {
                if (FulfillFromLeftover(buffer, index, ref count))
                {
                    return;
                }

                int num2 = Convert.ToBase64CharArray(_leftOverBytes, 0, 3, _charsLine, 0);
                await WriteCharsAsync(_charsLine, 0, num2, cancellationToken).ConfigureAwait(false);
            }

            StoreLeftOverBytes(buffer, index, ref count);

            int num4 = index + count;
            int length = LineSizeInBytes;
            while (index < num4)
            {
                if (index + length > num4)
                {
                    length = num4 - index;
                }
                int num6 = Convert.ToBase64CharArray(buffer, index, length, _charsLine, 0);
                await WriteCharsAsync(_charsLine, 0, num6, cancellationToken).ConfigureAwait(false);
                index += length;
            }
        }

        private Task WriteCharsAsync(char[] chars, int index, int count, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(chars, index, count, cancellationToken);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return cancellationToken.FromCanceled();
            }

            if (_leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(_leftOverBytes, 0, _leftOverBytesCount, _charsLine, 0);
                _leftOverBytesCount = 0;
                return WriteCharsAsync(_charsLine, 0, count, cancellationToken);
            }

            return AsyncUtils.CompletedTask;
        }

#endif
    }
}