//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Serialization;
using System;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// Xml序列化
    /// </summary>
    public class XmlSerializeConverter : SerializeConverter
    {
#pragma warning disable CS1591 // XML 注释

        public override object DeserializeParameter(byte[] parameterBytes, Type parameterType)
        {
            if (parameterBytes == null)
            {
                return null;
            }
            return SerializeConvert.XmlDeserializeFromBytes(parameterBytes, parameterType);
        }

        public override byte[] SerializeParameter(object parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            return SerializeConvert.XmlSerializeToBytes(parameter);
        }

#pragma warning restore CS1591
    }
}