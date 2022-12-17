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
using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket.Http
{
    /// <summary>
    /// 处理代理认证凭证
    /// </summary>
    internal class AuthenticationChallenge
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="value">服务器返回的凭证认证类型</param>
        /// <param name="credential">基本凭证用户名密码</param>
        /// <param name="nonceCount">暂时不知道是什么</param>
        public AuthenticationChallenge(string value, NetworkCredential credential, uint nonceCount = 0)
        {
            Parse(value, credential);
            NonceCount = nonceCount;
        }

        /// <summary>
        /// 暂时不知
        /// </summary>
        public uint NonceCount { get; set; }

        /// <summary>
        /// 其实用不用他都一样
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// 凭证类型
        /// </summary>
        public AuthenticationType Type { get; set; }

        /// <summary>
        /// 转换成凭证本文
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override string ToString()
        {
            if (Type == AuthenticationType.Basic)
                return ToBasicString();
            else
                throw new Exception("该凭证类型不支持");
        }

        private void Parse(string value, NetworkCredential credential)
        {
            var chal = value.Split(new[] { ' ' }, 2);
            if (chal.Length != 2)
                throw new Exception("该凭证类型不支持");

            var schm = chal[0].ToLower();
            Parameters = ParseParameters(chal[1]);

            if (Parameters.ContainsKey("username") == false)
                Parameters.Add("username", credential.Username);
            if (Parameters.ContainsKey("password") == false)
                Parameters.Add("password", credential.Password);

            /*
             *   Basic基本类型貌似只需要用户名密码即可
             *   if (this.parameters.ContainsKey("uri") == false)
                     this.parameters.Add("uri", credential.Domain);*/

            if (schm == "basic")
            {
                Type = AuthenticationType.Basic;
            }
            else
                throw new Exception("该凭证类型不支持");
        }

        private Dictionary<string, string> ParseParameters(string value)
        {
            var res = new Dictionary<string, string>();
            IEnumerable<string> values = SplitHeaderValue(value, ',');
            foreach (var param in values)
            {
                var i = param.IndexOf('=');
                var name = i > 0 ? param.Substring(0, i).Trim() : null;
                var val = i < 0
                          ? param.Trim().Trim('"')
                          : i < param.Length - 1
                            ? param.Substring(i + 1).Trim().Trim('"')
                            : string.Empty;

                res.Add(name, val);
            }
            return res;
        }

        private IEnumerable<string> SplitHeaderValue(string value, params char[] separators)
        {
            var len = value.Length;
            var end = len - 1;

            var buff = new StringBuilder(32);
            var escaped = false;
            var quoted = false;

            for (var i = 0; i <= end; i++)
            {
                var c = value[i];
                buff.Append(c);
                if (c == '"')
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }
                    quoted = !quoted;
                    continue;
                }
                if (c == '\\')
                {
                    if (i == end)
                        break;
                    if (value[i + 1] == '"')
                        escaped = true;
                    continue;
                }
                if (Array.IndexOf(separators, c) > -1)
                {
                    if (quoted)
                        continue;
                    buff.Length -= 1;
                    yield return buff.ToString();
                    buff.Length = 0;
                    continue;
                }
            }
            yield return buff.ToString();
        }

        private string ToBasicString()
        {
            var userPass = $"{Parameters["username"]}:{Parameters["password"]}";
            var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(userPass));
            return "Basic " + cred;
        }
    }
}