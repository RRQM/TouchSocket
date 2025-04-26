//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 固定包头数据包处理适配器，支持Byte、UShort、Int三种类型作为包头。使用<see cref="TouchSocketBitConverter.DefaultEndianType"/>大小端设置。
/// </summary>
public class FixedHeaderPackageAdapter : SingleStreamDataHandlingAdapter
{
    private byte[] m_agreementTempBytes;
    private int m_surPlusLength = 0;

    //包剩余长度
    private ByteBlock m_tempByteBlock;

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => false;

    /// <inheritdoc/>
    public override bool CanSplicingSend => true;

    /// <summary>
    /// 设置包头类型，默认为int
    /// </summary>
    public FixedHeaderType FixedHeaderType { get; set; } = FixedHeaderType.Int;

    /// <summary>
    /// 获取或设置包数据的最小值（默认为0）
    /// </summary>
    public int MinPackageSize { get; set; } = 0;

    /// <summary>
    /// 当接收到数据时处理数据
    /// </summary>
    /// <param name="byteBlock">数据流</param>
    protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
    {
        var array = byteBlock.Memory.GetArray();
        var buffer = array.Array;
        var r = byteBlock.Length;

        if (this.CacheTimeoutEnable && DateTimeOffset.UtcNow - this.LastCacheTime > this.CacheTimeout)
        {
            this.Reset();
        }

        if (this.m_agreementTempBytes != null)
        {
            await this.SeamPackage(buffer, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (this.m_tempByteBlock == null)
        {
            await this.SplitPackage(buffer, 0, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            if (this.m_surPlusLength == r)
            {
                this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                await this.PreviewHandle(this.m_tempByteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                this.m_tempByteBlock = null;
                this.m_surPlusLength = 0;
            }
            else if (this.m_surPlusLength < r)
            {
                this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                await this.PreviewHandle(this.m_tempByteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                this.m_tempByteBlock = null;
                await this.SplitPackage(buffer, this.m_surPlusLength, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, r));
                this.m_surPlusLength -= r;
                if (this.UpdateCacheTimeWhenRev)
                {
                    this.LastCacheTime = DateTimeOffset.UtcNow;
                }
            }
        }
    }

    private void ThrowIfLengthValidationFailed(int length)
    {
        if (length < this.MinPackageSize || length > this.MaxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(length), length, this.MinPackageSize, this.MaxPackageSize);
        }
    }


    /// <inheritdoc/>
    protected override Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
    {
        this.ThrowIfLengthValidationFailed(memory.Length);

        ByteBlock byteBlock;
        byte[] lenBytes;

        switch (this.FixedHeaderType)
        {
            case FixedHeaderType.Byte:
                {
                    var dataLen = (byte)(memory.Length);
                    byteBlock = new ByteBlock(dataLen + 1);
                    lenBytes = new byte[] { dataLen };
                    break;
                }
            case FixedHeaderType.Ushort:
                {
                    var dataLen = (ushort)(memory.Length);
                    byteBlock = new ByteBlock(dataLen + 2);
                    lenBytes = TouchSocketBitConverter.Default.GetBytes(dataLen);
                    break;
                }
            case FixedHeaderType.Int:
                {
                    var dataLen = memory.Length;
                    byteBlock = new ByteBlock(dataLen + 4);
                    lenBytes = TouchSocketBitConverter.Default.GetBytes(dataLen);
                    break;
                }
            default: throw new InvalidEnumArgumentException(TouchSocketCoreResource.InvalidParameter.Format(nameof(this.FixedHeaderType)));
        }

        try
        {
            byteBlock.Write(lenBytes);
            byteBlock.Write(memory.Span);
            await this.GoSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        if (transferBytes.Count == 0)
        {
            return;
        }

        var length = 0;
        foreach (var item in transferBytes)
        {
            length += item.Count;
        }

        this.ThrowIfLengthValidationFailed(length);

        ByteBlock byteBlock;
        byte[] lenBytes;

        switch (this.FixedHeaderType)
        {
            case FixedHeaderType.Byte:
                {
                    var dataLen = (byte)length;
                    byteBlock = new ByteBlock(dataLen + 1);
                    lenBytes = new byte[] { dataLen };
                    break;
                }
            case FixedHeaderType.Ushort:
                {
                    var dataLen = (ushort)length;
                    byteBlock = new ByteBlock(dataLen + 2);
                    lenBytes = TouchSocketBitConverter.Default.GetBytes(dataLen);
                    break;
                }
            case FixedHeaderType.Int:
                {
                    byteBlock = new ByteBlock(length + 4);
                    lenBytes = TouchSocketBitConverter.Default.GetBytes(length);
                    break;
                }
            default: throw new InvalidEnumArgumentException(TouchSocketCoreResource.InvalidParameter.Format(nameof(this.FixedHeaderType)));
        }

        try
        {
            byteBlock.Write(lenBytes);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
            }
            await this.GoSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <inheritdoc/>
    protected override void Reset()
    {
        this.m_agreementTempBytes = null;
        this.m_surPlusLength = default;
        this.m_tempByteBlock?.Dispose();
        this.m_tempByteBlock = null;
        base.Reset();
    }

    private async Task PreviewHandle(ByteBlock byteBlock)
    {
        try
        {
            byteBlock.Position = 0;
            await this.GoReceivedAsync(byteBlock, null).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 缝合包
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="r"></param>
    private async Task SeamPackage(byte[] buffer, int r)
    {
        var byteBlock = new ByteBlock(r + this.m_agreementTempBytes.Length);
        byteBlock.Write(this.m_agreementTempBytes);
        byteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, r));
        r += this.m_agreementTempBytes.Length;
        this.m_agreementTempBytes = null;

        var array = byteBlock.Memory.GetArray();
        var buffer2 = array.Array;
        await this.SplitPackage(buffer2, 0, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        byteBlock.Dispose();
    }

    /// <summary>
    /// 分解包
    /// </summary>
    /// <param name="dataBuffer"></param>
    /// <param name="index"></param>
    /// <param name="r"></param>
    private async Task SplitPackage(byte[] dataBuffer, int index, int r)
    {
        while (index < r)
        {
            if (r - index <= (byte)this.FixedHeaderType)
            {
                this.m_agreementTempBytes = new byte[r - index];
                Array.Copy(dataBuffer, index, this.m_agreementTempBytes, 0, this.m_agreementTempBytes.Length);
                if (this.UpdateCacheTimeWhenRev)
                {
                    this.LastCacheTime = DateTimeOffset.UtcNow;
                }
                return;
            }
            var length = 0;

            switch (this.FixedHeaderType)
            {
                case FixedHeaderType.Byte:
                    length = dataBuffer[index];
                    break;

                case FixedHeaderType.Ushort:
                    length = TouchSocketBitConverter.Default.ToUInt16(dataBuffer, index);
                    break;

                case FixedHeaderType.Int:
                    length = TouchSocketBitConverter.Default.ToInt32(dataBuffer, index);
                    break;
            }

            this.ThrowIfLengthValidationFailed(length);

            var recedSurPlusLength = r - index - (byte)this.FixedHeaderType;
            if (recedSurPlusLength >= length)
            {
                var byteBlock = new ByteBlock(length);
                byteBlock.Write(new ReadOnlySpan<byte>(dataBuffer, index + (byte)this.FixedHeaderType, length));
                await this.PreviewHandle(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                this.m_surPlusLength = 0;
            }
            else//半包
            {
                this.m_tempByteBlock = new ByteBlock(length);
                this.m_surPlusLength = length - recedSurPlusLength;
                this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(dataBuffer, index + (byte)this.FixedHeaderType, recedSurPlusLength));
                if (this.UpdateCacheTimeWhenRev)
                {
                    this.LastCacheTime = DateTimeOffset.UtcNow;
                }
            }
            index += (length + (byte)this.FixedHeaderType);
        }
    }
}