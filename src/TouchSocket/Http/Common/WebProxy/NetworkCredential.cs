using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 代理身份认证
    /// </summary>
    public class NetworkCredential
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain">基本认证应该不需要这个</param>
        /// <param name="roles"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public NetworkCredential(string username, string password, string domain, params string[] roles)
        {
            if (username == null)
                throw new ArgumentNullException("username");

            if (username.Length == 0)
                throw new ArgumentException("An empty string.", "username");
            Username = username;
            Password = password;
            Domain = domain;
            Roles = roles;
        }
        /// <summary>
        /// 凭证用户名
        /// </summary>

        public string Username { get; }
        /// <summary>
        /// 凭证密码
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Roles
        /// </summary>
        public string[] Roles { get; }
    }
}
