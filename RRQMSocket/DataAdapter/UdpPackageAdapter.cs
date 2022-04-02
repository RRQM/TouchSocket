﻿using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Data;
using RRQMCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// UDP数据帧
    /// </summary>
    public struct UdpFrame
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Crc校验
        /// </summary>
        public byte[] Crc { get; set; }

        /// <summary>
        /// 是否为终结帧
        /// </summary>
        public bool FIN { get; set; }

        /// <summary>
        /// 数据ID
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 帧序号
        /// </summary>
        public ushort SN { get; set; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool Parse(byte[] buffer, int offset, int length)
        {
            if (length > 11)
            {
                this.ID = RRQMBitConverter.Default.ToInt64(buffer, offset);
                this.SN = RRQMBitConverter.Default.ToUInt16(buffer, 8 + offset);
                this.FIN = buffer[10 + offset].GetBit(7) == 1;
                if (FIN)
                {
                    if (length > 13)
                    {
                        this.Data = new byte[length - 13];
                    }
                    else
                    {
                        this.Data = new byte[0];
                    }
                    this.Crc = new byte[2] { buffer[length - 2], buffer[length - 1] };
                }
                else
                {
                    this.Data = new byte[length - 11];
                }

                Array.Copy(buffer, 11, Data, 0, Data.Length);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// UDP数据包
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count={Count}")]
    public class UdpPackage
    {
        private int count;

        private ConcurrentQueue<UdpFrame> frames;

        private int totalCount = -1;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <param name="revStore"></param>
        public UdpPackage(long id, int timeout, ConcurrentDictionary<long, UdpPackage> revStore)
        {
            this.ID = id;
            this.frames = new ConcurrentQueue<UdpFrame>();
            this.timer = new Timer((o) =>
            {
                if (revStore.TryRemove(this.ID, out UdpPackage udpPackage))
                {
                    udpPackage.frames.Clear();
                }
            }, null, timeout, Timeout.Infinite);
        }

        readonly Timer timer;

        /// <summary>
        /// 当前长度
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// 包唯一标识
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsComplated { get => totalCount > 0 ? (totalCount == count ? true : false) : false; }

        /// <summary>
        /// 总长度，在收到最后一帧之前，为-1。
        /// </summary>
        public int TotalCount
        {
            get { return totalCount; }
        }

        /// <summary>
        /// Crc
        /// </summary>
        public byte[] Crc { get; private set; }

        private int mtu;

        /// <summary>
        /// MTU
        /// </summary>
        public int MTU
        {
            get { return mtu + 11; }
        }

        private int length;
        /// <summary>
        /// 当前数据长度
        /// </summary>
        public int Length
        {
            get { return length; }
        }


        /// <summary>
        /// 添加帧
        /// </summary>
        /// <param name="frame"></param>
        public void Add(UdpFrame frame)
        {
            Interlocked.Increment(ref count);

            if (frame.FIN)
            {
                this.totalCount = frame.SN + 1;
                this.Crc = frame.Crc;
            }
            Interlocked.Add(ref length, frame.Data.Length);
            if (frame.SN == 0)
            {
                this.mtu = frame.Data.Length;
            }
            this.frames.Enqueue(frame);
        }

        /// <summary>
        /// 获得数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public bool TryGetData(ByteBlock byteBlock)
        {
            while (this.frames.TryDequeue(out UdpFrame frame))
            {
                byteBlock.Pos = frame.SN * this.mtu;
                byteBlock.Write(frame.Data);
            }

            if (byteBlock.Len != this.Length)
            {
                return false;
            }
            var crc = RRQMCore.Data.Crc.Crc16(byteBlock.Buffer, 0, byteBlock.Len);
            if (crc[0] != this.Crc[0] || crc[1] != this.Crc[1])
            {
                return false;
            }

            return true;
        }

    }

    /// <summary>
    /// UDP数据包的适配器
    /// </summary>
    public class UdpPackageAdapter : UdpDataHandlingAdapter
    {
        private readonly ConcurrentDictionary<long, UdpPackage> revStore;

        private SnowflakeIDGenerator iDGenerator;

        private int mtu = 1472;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpPackageAdapter()
        {
            this.revStore = new ConcurrentDictionary<long, UdpPackage>();
            this.iDGenerator = new SnowflakeIDGenerator(4);
        }

        /// <summary>
        /// 接收超时时间，默认5000ms
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

        /// <summary>
        /// 最大传输单元
        /// </summary>
        public int MTU
        {
            get => this.mtu + 11;
            set => this.mtu = value > 11 ? value : 1472;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            UdpFrame udpFrame = new UdpFrame();
            if (udpFrame.Parse(byteBlock.Buffer, 0, byteBlock.Len))
            {
                UdpPackage udpPackage = revStore.GetOrAdd(udpFrame.ID, (i) => new UdpPackage(i, this.Timeout, this.revStore));
                udpPackage.Add(udpFrame);
                if (udpPackage.Length > this.MaxPackageSize)
                {
                    this.revStore.TryRemove(udpPackage.ID, out _);
                    this.owner?.Logger.Error("数据长度大于设定的最大值。");
                    return;
                }
                if (udpPackage.IsComplated)
                {
                    if (this.revStore.TryRemove(udpPackage.ID, out _))
                    {
                        using (ByteBlock block = new ByteBlock(udpPackage.Length))
                        {
                            if (udpPackage.TryGetData(block))
                            {
                                this.GoReceived(remoteEndPoint, block, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length, bool isAsync)
        {
            if (length > this.MaxPackageSize)
            {
                throw new RRQMOverlengthException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }
            lock (this)
            {
                long id = iDGenerator.NextID();
                int off = 0;
                int surLen = length;
                int freeRoom = this.mtu - 11;
                ushort sn = 0;
                /*|********|**|*|n|*/
                /*|********|**|*|**|*/
                while (surLen > 0)
                {
                    byte[] data = new byte[this.mtu];
                    Buffer.BlockCopy(RRQMBitConverter.Default.GetBytes(id), 0, data, 0, 8);
                    Buffer.BlockCopy(RRQMBitConverter.Default.GetBytes(sn++), 0, data, 8, 2);
                    if (surLen > freeRoom)//有余
                    {
                        Buffer.BlockCopy(buffer, off, data, 11, freeRoom);
                        off += freeRoom;
                        surLen -= freeRoom;
                        this.GoSend(endPoint, data, 0, this.mtu, isAsync);
                    }
                    else if (surLen + 2 <= freeRoom)//结束且能容纳Crc
                    {
                        byte flag = 0;
                        data[10] = flag.SetBit(7, 1);//设置终结帧

                        Buffer.BlockCopy(buffer, off, data, 11, surLen);
                        Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, data, 11 + surLen, 2);

                        this.GoSend(endPoint, data, 0, surLen + 11 + 2, isAsync);

                        off += surLen;
                        surLen -= surLen;

                    }
                    else//结束但不能容纳Crc
                    {
                        Buffer.BlockCopy(buffer, off, data, 11, surLen);
                        this.GoSend(endPoint, data, 0, surLen + 11, isAsync);
                        off += surLen;
                        surLen -= surLen;

                        byte[] finData = new byte[13];
                        Buffer.BlockCopy(RRQMBitConverter.Default.GetBytes(id), 0, finData, 0, 8);
                        Buffer.BlockCopy(RRQMBitConverter.Default.GetBytes(sn++), 0, finData, 8, 2);
                        byte flag = 0;
                        finData[10] = flag.SetBit(7, 1);
                        Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, finData, 11, 2);
                        this.GoSend(endPoint, finData, 0, finData.Length, isAsync);
                    }
                }


            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(EndPoint endPoint, IList<TransferByte> transferBytes, bool isAsync)
        {
            int length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Length;
            }

            if (length > this.MaxPackageSize)
            {
                throw new RRQMOverlengthException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            using (ByteBlock byteBlock = new ByteBlock(length))
            {
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(item.Buffer, item.Offset, item.Length);
                }
                this.PreviewSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len, isAsync);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
        }
    }
}