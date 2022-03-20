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

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 生成的代码单元
    /// </summary>

    public class MethodCellCode
    {
        /// <summary>
        /// 方法名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 代码本体
        /// </summary>
        public string CodeTemple { get; set; }

        /// <summary>
        /// 调用唯一键
        /// </summary>
        public string InvokeKey { get; set; }

        /// <summary>
        /// 获取实际代码
        /// </summary>
        /// <returns></returns>
        public string GetCode()
        {
          return  this.CodeTemple.Replace("{0}",this.Name)
                .Replace("{1}", string.IsNullOrEmpty(this.InvokeKey) ? this.Name : this.InvokeKey);
        }

        /// <summary>
        /// 接口代码。
        /// </summary>
        public string InterfaceTemple { get; set; }

        /// <summary>
        /// 获取实际接口代码
        /// </summary>
        /// <returns></returns>
        public string GetInterfaceCode()
        {
            return string.Format(this.InterfaceTemple, this.Name);
        }
    }
}