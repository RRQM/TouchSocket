namespace RRQMSocket
{
    /// <summary>
    /// 返回通知接口
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 消息
        /// </summary>
        string Message { get; }
    }
}
