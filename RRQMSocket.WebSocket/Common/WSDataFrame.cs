//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket数据帧
    /// </summary>
    public class WSDataFrame : IDisposable
    {
        /// <summary>
        /// 是否为最后数据帧。
        /// </summary>
        public bool FIN { get; set; }

        /// <summary>
        /// 标识RSV-1。
        /// </summary>
        public bool RSV1 { get; set; }

        /// <summary>
        /// 标识RSV-2。
        /// </summary>
        public bool RSV2 { get; set; }

        /// <summary>
        /// 标识RSV-3。
        /// </summary>
        public bool RSV3 { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public WSDataType Opcode { get; set; }

        /// <summary>
        /// 计算掩码
        /// </summary>
        public bool Mask { get; set; }

        /// <summary>
        /// 有效载荷数据长度
        /// </summary>
        public int PayloadLength { get; set; }

        /// <summary>
        /// 掩码值
        /// </summary>
        public byte[] MaskingKey { get; set; }

        /// <summary>
        /// 有效数据
        /// </summary>
        public ByteBlock PayloadData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (this.PayloadData != null)
            {
                this.PayloadData.Dispose();
            }
        }

        /// <summary>
        /// 构建数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="masked"></param>
        /// <returns></returns>
        public bool Build(ByteBlock byteBlock, bool masked)
        {
            if (this.PayloadData != null)
            {
                this.PayloadLength = PayloadData.Len;
            }
            else
            {
                this.PayloadLength = 0;
            }

            if (this.PayloadData==null)
            {
                return WSTools.Build(byteBlock, this, new byte[0], 0, 0);
            }
            return WSTools.Build(byteBlock, this, this.PayloadData.Buffer, 0, this.PayloadLength);
        }
    }
}
