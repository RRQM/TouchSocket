using System.Collections.Generic;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http参数
    /// </summary>
    public interface IHttpParams : IDictionary<string, string>
    {
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(string key);
    }
}
