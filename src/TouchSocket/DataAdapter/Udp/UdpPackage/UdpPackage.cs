using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// UDP数据包
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count={Count}")]
    public class UdpPackage
    {
        private readonly ConcurrentQueue<UdpFrame> m_frames;
        private readonly Timer m_timer;
        private int m_count;
        private int m_length;

        private int m_mtu;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="revStore"></param>
        public UdpPackage(long id, int millisecondsTimeout, ConcurrentDictionary<long, UdpPackage> revStore)
        {
            this.Id = id;
            this.m_frames = new ConcurrentQueue<UdpFrame>();
            this.m_timer = new Timer((o) =>
            {
                if (revStore.TryRemove(this.Id, out var udpPackage))
                {
                    udpPackage.m_frames.Clear();
                }
            }, null, millisecondsTimeout, Timeout.Infinite);
        }

        /// <summary>
        /// 当前长度
        /// </summary>
        public int Count => this.m_count;

        /// <summary>
        /// Crc
        /// </summary>
        public byte[] Crc { get; private set; }

        /// <summary>
        /// 包唯一标识
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsComplated => this.TotalCount > 0 && (this.TotalCount == this.m_count);

        /// <summary>
        /// 当前数据长度
        /// </summary>
        public int Length => this.m_length;

        /// <summary>
        /// MTU
        /// </summary>
        public int MTU => this.m_mtu + 11;

        /// <summary>
        /// 总长度，在收到最后一帧之前，为-1。
        /// </summary>
        public int TotalCount { get; private set; } = -1;

        /// <summary>
        /// 添加帧
        /// </summary>
        /// <param name="frame"></param>
        public void Add(UdpFrame frame)
        {
            Interlocked.Increment(ref this.m_count);

            if (frame.FIN)
            {
                this.TotalCount = frame.SN + 1;
                this.Crc = frame.Crc;
            }
            Interlocked.Add(ref this.m_length, frame.Data.Length);
            if (frame.SN == 0)
            {
                this.m_mtu = frame.Data.Length;
            }
            this.m_frames.Enqueue(frame);
        }

        /// <summary>
        /// 获得数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public bool TryGetData(ByteBlock byteBlock)
        {
            while (this.m_frames.TryDequeue(out var frame))
            {
                byteBlock.Position = frame.SN * this.m_mtu;
                byteBlock.Write(frame.Data);
            }

            if (byteBlock.Length != this.Length)
            {
                return false;
            }
            var crc = Core.Crc.Crc16(byteBlock.Span);
            return crc[0] == this.Crc[0] && crc[1] == this.Crc[1];
        }
    }
}
