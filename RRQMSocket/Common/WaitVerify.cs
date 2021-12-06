using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 验证消息
    /// </summary>
    public class WaitVerify : WaitResult
    {
        /// <summary>
        /// 令箭
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 转化数据
        /// </summary>
        /// <returns></returns>
        internal byte[] GetData()
        {
            return RRQMCore.Data.Security.DataLock.EncryptDES(SerializeConvert.RRQMBinarySerialize(this, true), "RRQMRRQM");
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal static WaitVerify GetVerifyInfo(byte[] buffer)
        {
            return SerializeConvert.RRQMBinaryDeserialize<WaitVerify>(RRQMCore.Data.Security.DataLock.DecryptDES(buffer, "RRQMRRQM"));
        }
    }
}
