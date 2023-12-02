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

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 密封的<see cref="DmtpActor"/>
    /// </summary>
    public sealed class SealedDmtpActor : DmtpActor
    {
        /// <summary>
        /// 创建一个Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute">是否允许路由</param>
        /// <param name="isReliable">是不是基于可靠协议运行的</param>
        public SealedDmtpActor(bool allowRoute, bool isReliable) : base(allowRoute, isReliable)
        {
        }

        /// <summary>
        /// 创建一个可靠协议的Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute"></param>
        public SealedDmtpActor(bool allowRoute) : this(allowRoute, true)
        {
        }
    }
}