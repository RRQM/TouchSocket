namespace TouchSocket.Http
{
    /// <summary>
    /// IContentTypeProvider
    /// </summary>
    public interface IContentTypeProvider
    {
        /// <summary>
        /// 给定文件路径，确定MIME类型
        /// </summary>
        /// <param name="subpath"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        bool TryGetContentType(string subpath, out string contentType);
    }
}