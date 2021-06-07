using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 操作属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OperationAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OperationAttribute()
        { 
        
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="operationName"></param>
        public OperationAttribute(string operationName)
        {
            this.OperationName = operationName;
        }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string OperationName { get;private set; }
    }
}
