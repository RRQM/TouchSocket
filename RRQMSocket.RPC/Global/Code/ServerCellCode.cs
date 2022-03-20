//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Generic;

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


        private Dictionary<string, MethodCellCode> methods;

        /// <summary>
        /// 方法集合
        /// </summary>
        public Dictionary<string, MethodCellCode> Methods
        {
            get => this.methods;
            set => this.methods = value;
        }

        private Dictionary<string, ClassCellCode> classCellCodes;

        /// <summary>
        /// 类参数集合。
        /// </summary>
        public Dictionary<string, ClassCellCode> ClassCellCodes
        {
            get => this.classCellCodes;
            set => this.classCellCodes = value;
        }
    }
}