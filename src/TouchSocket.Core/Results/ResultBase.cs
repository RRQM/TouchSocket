using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 结果返回
    /// </summary>
    public class ResultBase : IResult
    {

        /// <summary>
        /// 初始化 ResultBase 类的新实例。
        /// </summary>
        /// <param name="resultCode">结果代码，表示操作的结果。</param>
        /// <param name="message">消息，提供有关操作结果的详细信息。</param>
        public ResultBase(ResultCode resultCode, string message)
        {
            this.ResultCode = resultCode;
            this.Message = message;
        }


        /// <summary>
        /// 初始化 ResultBase 类的新实例。
        /// </summary>
        /// <param name="resultCode">结果代码，表示操作的结果。</param>
        public ResultBase(ResultCode resultCode)
        {
            // 将传入的结果代码赋值给类的 ResultCode 属性。
            this.ResultCode = resultCode;

            // 根据结果代码获取其描述信息，并赋值给 Message 属性。
            this.Message = resultCode.GetDescription();
        }

        /// <summary>
        /// 初始化 ResultBase 类的新实例。
        /// </summary>
        /// <param name="result">要复制其属性值的 Result 对象。</param>
        public ResultBase(Result result)
        {
            // 复制传入 Result 对象的 ResultCode 属性值。
            this.ResultCode = result.ResultCode;

            // 复制传入 Result 对象的 Message 属性值。
            this.Message = result.Message;
        }


        /// <summary>
        /// ResultBase 类的构造函数。
        /// </summary>
        public ResultBase()
        {
        }

        /// <inheritdoc/>
        public ResultCode ResultCode { get; protected set; }

        /// <inheritdoc/>
        public string Message { get; protected set; }

        /// <inheritdoc/>
        public bool IsSuccess => this.ResultCode == ResultCode.Success;

        /// <inheritdoc/>
        public override string ToString()
        {
            return TouchSocketCoreResource.ResultToString.Format(this.ResultCode, this.Message);
        }
    }
}
