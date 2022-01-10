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
using RRQMCore.Helper;
using RRQMCore.Serialization;
using RRQMCore.XREF.Newtonsoft.Json;
using System;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 默认序列化选择器
    /// </summary>
    public class DefaultSerializationSelector : SerializationSelector
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameterBytes"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public override object DeserializeParameter(SerializationType serializationType, byte[] parameterBytes, Type parameterType)
        {
            if (parameterBytes == null)
            {
                return parameterType.GetDefault();
            }
            switch (serializationType)
            {
                case SerializationType.RRQMBinary:
                    {
                        return SerializeConvert.RRQMBinaryDeserialize(parameterBytes, 0, parameterType);
                    }
                case SerializationType.Json:
                    {
                        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(parameterBytes), parameterType);
                    }
                case SerializationType.Xml:
                    {
                        return SerializeConvert.XmlDeserializeFromBytes(parameterBytes, parameterType);
                    }
                default:
                    throw new RRQMRPCException("未指定的反序列化方式");
            }
        }

        /// <summary>
        /// 序列化参数
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override byte[] SerializeParameter(SerializationType serializationType, object parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            switch (serializationType)
            {
                case SerializationType.RRQMBinary:
                    {
                        return SerializeConvert.RRQMBinarySerialize(parameter);
                    }
                case SerializationType.Json:
                    {
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameter));
                    }
                case SerializationType.Xml:

                    {
                        return SerializeConvert.XmlSerializeToBytes(parameter);
                    }
                default:
                    throw new RRQMRPCException("未指定的序列化方式");
            }
        }
    }
}