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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 响应数据。
    /// </summary>
    public struct ResponsedData
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="requestInfo"></param>
        /// <param name="isRawBuffer"></param>
        public ResponsedData(byte[] data, IRequestInfo requestInfo, bool isRawBuffer)
        {
            this.Data = data;
            this.RequestInfo = requestInfo;
            this.IsRawBuffer = isRawBuffer;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// RequestInfo
        /// </summary>
        public IRequestInfo RequestInfo { get; private set; }

        /// <summary>
        /// 是否为原生缓存区。即没有经过适配器处理。
        /// </summary>
        public bool IsRawBuffer { get; private set; }
    }
}