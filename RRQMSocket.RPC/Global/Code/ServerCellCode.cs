using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务单元代码
    /// </summary>
    public class ServerCellCode
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServerCellCode()
        {
            this.methods = new Dictionary<string, MethodCellCode>();
            this.classCellCodes = new Dictionary<string, ClassCellCode>();
        }
        /// <summary>
        /// 服务名
        /// </summary>
        public string Name { get; set; }

#if NET45_OR_GREATER
        /// <summary>
        /// 引用
        /// </summary>
        public string[] Refs { get; set; }
#endif

        private Dictionary<string, MethodCellCode> methods;

        /// <summary>
        /// 方法集合
        /// </summary>
        public Dictionary<string, MethodCellCode> Methods
        {
            get { return methods; }
            set { methods = value; }
        }

        private Dictionary<string, ClassCellCode> classCellCodes;
       
        /// <summary>
        /// 类参数集合。
        /// </summary>
        public Dictionary<string, ClassCellCode> ClassCellCodes
        {
            get { return classCellCodes; }
            set { classCellCodes = value; }
        }

    }
}
