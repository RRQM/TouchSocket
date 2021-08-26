using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 序列化选择器
    /// </summary>
    public abstract class SerializationSelector
    {
        /// <summary>
        /// 序列化RPC方法返回值参数
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public abstract byte[] SerializeParameter(SerializationType serializationType, object parameter);

        /// <summary>
        /// 反序列化传输对象
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameterBytes"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public abstract object DeserializeParameter(SerializationType serializationType, byte[] parameterBytes, Type parameterType);
    }
}
