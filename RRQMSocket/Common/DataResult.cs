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
using RRQMCore;

namespace RRQMSocket
{
    /// <summary>
    /// 数据处理结果
    /// </summary>
    public struct DataResult
    {
        /// <summary>
        /// 错误
        /// </summary>
        public static DataResult ErrorResult;

        /// <summary>
        /// 不处理
        /// </summary>
        public static DataResult CacheResult;

        /// <summary>
        /// 成功
        /// </summary>
        public static DataResult SuccessResult;

        private string message;

        private DataResultCode resultCode;

        static DataResult()
        {
            SuccessResult = new DataResult(null, DataResultCode.Success);
            ErrorResult = new DataResult(ResType.UnknownError.GetResString(), DataResultCode.Error);
            CacheResult = new DataResult(null, DataResultCode.Cache);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="resultCode"></param>
        public DataResult(string message, DataResultCode resultCode)
        {
            this.message = message;
            this.resultCode = resultCode;
        }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message
        {
            get { return this.message; }
        }

        /// <summary>
        /// 结果类型
        /// </summary>
        public DataResultCode ResultCode
        {
            get { return this.resultCode; }
        }
    }
}