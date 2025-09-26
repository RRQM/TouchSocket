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

using System.Collections.Concurrent;
using System.Net;

namespace TouchSocket.Core;

/// <summary>
/// UDP数据包的适配器
/// </summary>
public class UdpPackageAdapter : UdpDataHandlingAdapter
{
    private readonly ConcurrentDictionary<long, UdpPackage> m_revStore;
    private int m_mtu = 1472;

    /// <summary>
    /// 构造函数
    /// </summary>
    public UdpPackageAdapter()
    {
        this.m_revStore = new ConcurrentDictionary<long, UdpPackage>();
    }

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => false;

    /// <summary>
    /// 最大传输单元
    /// </summary>
    public int MTU
    {
        get => this.m_mtu + 11;
        set => this.m_mtu = value > 11 ? value : 1472;
    }

    /// <summary>
    /// 接收超时时间，默认5000ms
    /// </summary>
    public int Timeout { get; set; } = 5000;

    /// <inheritdoc/>
    protected override async Task PreviewReceivedAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory)
    {
        var udpFrame = new UdpFrame();
        if (udpFrame.Parse(memory.Span))
        {
            var udpPackage = this.m_revStore.GetOrAdd(udpFrame.Id, (i) => new UdpPackage(i, this.Timeout, this.m_revStore));
            udpPackage.Add(udpFrame);
            if (udpPackage.Length > this.MaxPackageSize)
            {
                this.m_revStore.TryRemove(udpPackage.Id, out _);

                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(udpPackage.Length), udpPackage.Length, this.MaxPackageSize);
                return;
            }
            if (udpPackage.IsComplated)
            {
                if (this.m_revStore.TryRemove(udpPackage.Id, out _))
                {
                    using (var block = new ByteBlock(udpPackage.Length))
                    {
                        if (udpPackage.TryGetData(block))
                        {
                            await this.GoReceived(remoteEndPoint, block.Memory, null);
                        }
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.ThrowIfMoreThanMaxPackageSize(memory.Length);

        var id = TouchSocketCoreUtility.GenerateRandomInt64();
        var off = 0;
        var surLen = memory.Length;
        var freeRoom = this.m_mtu - 11;
        ushort sn = 0;
        /*|********|**|*|n|*/
        /*|********|**|*|**|*/
        while (surLen > 0)
        {
            var byteBlock = new ByteBlock(this.m_mtu);
            try
            {
                WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, id);
                WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, sn++);

                if (surLen > freeRoom)//有余
                {
                    WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, 0);
                    byteBlock.Write(memory.Span.Slice(off, freeRoom));
                    //Buffer.BlockCopy(buffer, off, data, 11, freeRoom);
                    off += freeRoom;
                    surLen -= freeRoom;
                    await this.GoSendAsync(endPoint, byteBlock.Memory, cancellationToken);
                }
                else if (surLen + 2 <= freeRoom)//结束且能容纳Crc
                {

                    var flag = ((byte)0).SetBit(7, true);//设置终结帧
                    WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, flag);

                    byteBlock.Write(memory.Span.Slice(off, surLen));
                    //Buffer.BlockCopy(buffer, off, data, 11, surLen);

                    WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, Crc.Crc16Value(memory.Span), EndianType.Big);
                    //Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, data, 11 + surLen, 2);

                    //Buffer.BlockCopy(Crc.Crc16(memory.Span), 0, data, 11 + surLen, 2);

                    //await this.GoSendAsync(endPoint, data, 0, surLen + 11 + 2);
                    await this.GoSendAsync(endPoint, byteBlock.Memory, cancellationToken);

                    off += surLen;
                    surLen -= surLen;
                }
                else//结束但不能容纳Crc
                {
                    WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, 0);
                    byteBlock.Write(memory.Span.Slice(off, surLen));
                    //Buffer.BlockCopy(buffer, off, data, 11, surLen);
                    await this.GoSendAsync(endPoint, byteBlock.Memory, cancellationToken);

                    byteBlock.Reset();

                    off += surLen;
                    surLen -= surLen;

                    //var finData = new byte[13];

                    WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, id);
                    WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, sn++);
                    WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, ((byte)0).SetBit(7, true));
                    WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, Crc.Crc16Value(memory.Span), EndianType.Big);

                    //Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(id), 0, finData, 0, 8);
                    //Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(sn++), 0, finData, 8, 2);
                    //byte flag = 0;
                    //finData[10] = flag.SetBit(7, 1);
                    //Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, finData, 11, 2);
                    await this.GoSendAsync(endPoint, byteBlock.Memory, cancellationToken);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}