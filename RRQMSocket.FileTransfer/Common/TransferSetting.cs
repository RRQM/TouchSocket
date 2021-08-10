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
using System;

namespace RRQMSocket.FileTransfer
{
    internal class TransferSetting
    {
        internal bool breakpointResume;
        internal int bufferLength;

        internal void Serialize(ByteBlock byteBlock)
        {
            byteBlock.Write(Convert.ToByte(breakpointResume));
            byteBlock.Write(BitConverter.GetBytes(bufferLength));
        }

        internal static TransferSetting Deserialize(byte[] buffer, int offset)
        {
            TransferSetting transferSetting = new TransferSetting();
            transferSetting.breakpointResume = BitConverter.ToBoolean(buffer, offset);
            transferSetting.bufferLength = BitConverter.ToInt32(buffer, offset + 1);
            return transferSetting;
        }
    }
}