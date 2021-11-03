namespace RRQMSocket
{
    /// <summary>
    /// 结果实现类
    /// </summary>
    public class AsyncResult : IResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        public AsyncResult(bool isSuccess, string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        public AsyncResult(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public AsyncResult(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsSuccess { get; private set; }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Message { get; private set; }


    }
}
