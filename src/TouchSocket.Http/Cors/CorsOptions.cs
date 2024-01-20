using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 跨域相关配置
    /// </summary>
    public class CorsOptions
    {
        private readonly Dictionary<string, CorsPolicy> m_corsPolicys = new Dictionary<string, CorsPolicy>();

        /// <summary>
        /// 跨域策略集
        /// </summary>
        public Dictionary<string, CorsPolicy> CorsPolicys => this.m_corsPolicys;

        /// <summary>
        /// 添加跨域策略
        /// </summary>
        /// <param name="policyName"></param>
        /// <param name="corsBuilderAction"></param>
        public void Add(string policyName, Action<CorsBuilder> corsBuilderAction)
        {
            var corsBuilder = new CorsBuilder();
            corsBuilderAction.Invoke(corsBuilder);
            m_corsPolicys.Add(policyName, corsBuilder.Build());
        }

        /// <summary>
        /// 添加跨域策略
        /// </summary>
        /// <param name="policyName"></param>
        /// <param name="corsResult"></param>
        public void Add(string policyName, CorsPolicy corsResult)
        {
            m_corsPolicys.Add(policyName, corsResult);
        }
    }
}