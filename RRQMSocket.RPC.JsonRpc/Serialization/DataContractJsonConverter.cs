using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// 使用DataContractJson转化器
    /// </summary>
    public class DataContractJsonConverter : JsonConverter
    {
#pragma warning disable CS1591 
        public override object Deserialize(string jsonString, Type parameterType)
        {
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(parameterType);
            return deseralizer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(jsonString)));
        }


        public override void Serialize(Stream stream, object parameter)
        {
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(parameter.GetType());
            deseralizer.WriteObject(stream, parameter);
        }
    }
}
