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
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{

    /// <summary>
    /// 过滤结果
    /// </summary>
    public enum FilterResult
    {
        /// <summary>
        /// 缓存后续所有<see cref="ByteBlock.CanReadLen"/>数据。
        /// </summary>
        Cache,

        /// <summary>
        /// 操作成功
        /// </summary>
        Success,

        /// <summary>
        /// 继续操作，一般原因是本次数据有部分无效，但已经调整了<see cref="ByteBlock.Pos"/>属性，所以继续后续解析。
        /// <para>或者想放弃当前数据的操作，直接设置<see cref="ByteBlock.Pos"/>与<see cref="ByteBlock.Len"/>相等即可。</para>
        /// </summary>
        GoOn
    }
}
