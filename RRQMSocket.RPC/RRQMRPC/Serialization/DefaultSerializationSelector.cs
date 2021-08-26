using RRQMCore.Serialization;
using RRQMCore.XREF.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return null;
            }
            switch (serializationType)
            {
                case SerializationType.RRQMBinary:
                    {
                        return SerializeConvert.RRQMBinaryDeserialize(parameterBytes, 0, parameterType);
                    }
                case SerializationType.SystemBinary:
                    {
                        return SerializeConvert.BinaryDeserialize(parameterBytes, 0, parameterBytes.Length);
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
                        return SerializeConvert.RRQMBinarySerialize(parameter, true);
                    }
                case SerializationType.SystemBinary:
                    {
                        return SerializeConvert.BinarySerialize(parameter);
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
