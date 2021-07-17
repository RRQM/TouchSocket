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
using RRQMCore.Helper;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// 使用DataContractJson转化器
    /// </summary>
    public class DataContractJsonConverter : JsonFormatConverter
    {
#pragma warning disable CS1591

        public override object Deserialize(string jsonString, Type parameterType)
        {
            if (parameterType.IsPrimitive||parameterType==typeof(string))
            {
                return jsonString.ParseToType(parameterType);
            }
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(parameterType);
            return deseralizer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(jsonString)));
        }

        public override string Serialize(object parameter)
        {
            using (MemoryStream stream =new MemoryStream())
            {
                DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(parameter.GetType());
                deseralizer.WriteObject(stream, parameter);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            
        }
    }
}