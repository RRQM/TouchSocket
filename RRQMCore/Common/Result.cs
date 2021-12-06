using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore
{
    /// <summary>
    /// 结果返回
    /// </summary>
    public struct Result:IResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        public Result(ResultCode resultCode, string message )
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
            this.Message =resultCode.GetResString();
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
            return $"类型：{ResultCode}，信息：{Message}";
        }
    }
}
