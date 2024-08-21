using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// ResultExtensions
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [Obsolete("建议使用IsSuccess属性代替该扩展方法")]
        public static bool IsSuccess(this IResult result)
        {
            return result.ResultCode == ResultCode.Success;
        }

        /// <summary>
        /// 是否没有成功。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [Obsolete("建议使用IsSuccess属性取反代替该扩展方法")]
        public static bool NotSuccess(this IResult result)
        {
            return result.ResultCode != ResultCode.Success;
        }

        /// <summary>
        /// 转换为<see cref="Result"/>
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Result ToResult(this IResult result)
        {
            return new Result(result);
        }
    }
}
