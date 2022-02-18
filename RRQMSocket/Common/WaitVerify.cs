//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Run;
using RRQMCore.Serialization;

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

        internal bool Handle;

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
            return RRQMCore.Data.Security.DataLock.EncryptDES(SerializeConvert.RRQMBinarySerialize(this), "RRQMRRQM");
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