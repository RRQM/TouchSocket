using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示http的headers
    /// </summary>
    public interface IHttpHeader: IDictionary<string, string>
    {
        /// <summary>
        /// 获取Header
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// 获取Header
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(HttpHeaders key);

        /// <summary>
        /// 添加Header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(HttpHeaders key,string value);

        /// <summary>
        /// 获取、添加Header
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        string this[HttpHeaders headers] { get;set; }
    }
}
