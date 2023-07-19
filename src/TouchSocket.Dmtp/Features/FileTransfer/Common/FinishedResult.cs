//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 完成的请求结果
    /// </summary>
    public class FinishedResult : ResultBase
    {
        private readonly int[] m_unFinishedIndexs;

        /// <summary>
        /// 失败的请求结果
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="resourceHandle"></param>
        /// <param name="unFinishedIndexs"></param>
        public FinishedResult(ResultCode resultCode, string message, int resourceHandle, int[] unFinishedIndexs) :
            base(resultCode, message)
        {
            this.ResourceHandle = resourceHandle;
            this.m_unFinishedIndexs = unFinishedIndexs;
        }

        /// <summary>
        /// 完成的请求结果
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="resourceHandle"></param>
        /// <param name="unFinishedIndexs"></param>
        public FinishedResult(ResultCode resultCode, int resourceHandle, int[] unFinishedIndexs) :
          base(resultCode)
        {
            this.ResourceHandle = resourceHandle;
            this.m_unFinishedIndexs = unFinishedIndexs;
        }

        /// <summary>
        /// 失败的请求结果
        /// </summary>
        /// <param name="result"></param>
        /// <param name="resourceHandle"></param>
        public FinishedResult(Result result, int resourceHandle) :
         base(result)
        {
            this.ResourceHandle = resourceHandle;
            this.m_unFinishedIndexs = new int[0];
        }

        /// <summary>
        /// 资源句柄
        /// </summary>
        public int ResourceHandle { get; private set; }

        /// <summary>
        /// 未完成的文件块集合
        /// </summary>
        public int[] UnFinishedIndexs { get => this.m_unFinishedIndexs ?? new int[0]; }
    }
}