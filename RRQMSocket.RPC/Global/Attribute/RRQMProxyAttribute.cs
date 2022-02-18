using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 代理类
    /// </summary>
    public class RRQMProxyAttribute : Attribute
    {
        private string className;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="className"></param>
        public RRQMProxyAttribute(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException($"“{nameof(className)}”不能为 null 或空。", nameof(className));
            }

            this.className = className;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMProxyAttribute()
        { 
        
        }

        /// <summary>
        /// 代理类名
        /// </summary>
        public string ClassName
        {
            get { return className; }
            set { className = value; }
        }

    }
}
