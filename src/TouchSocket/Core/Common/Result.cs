//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace TouchSocket.Core
{
    /// <summary>
    /// 结果返回
    /// </summary>
    public struct Result : IResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        public Result(ResultCode resultCode, string message)
        {
            this.ResultCode = resultCode;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resultCode"></param>
        public Result(ResultCode resultCode)
        {
            this.ResultCode = resultCode;
            this.Message = resultCode.GetDescription();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ResultCode ResultCode { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"类型：{this.ResultCode}，信息：{this.Message}";
        }
    }

    /// <summary>
    /// 结果返回
    /// </summary>
    public class ResultBase : IResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        public ResultBase(ResultCode resultCode, string message)
        {
            this.ResultCode = resultCode;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resultCode"></param>
        public ResultBase(ResultCode resultCode)
        {
            this.ResultCode = resultCode;
            this.Message = resultCode.GetDescription();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ResultCode ResultCode { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"类型：{this.ResultCode}，信息：{this.Message}";
        }
    }
}