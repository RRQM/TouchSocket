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

using System.ComponentModel;

namespace TouchSocket.Resources
{
    /// <summary>
    /// TouchSocket资源枚举
    /// </summary>
    public enum TouchSocketResource
    {
        /// <summary>
        /// 没有找到Id为{0}的客户端。
        /// </summary>
        [Description("没有找到Id为{0}的客户端。")]
        ClientNotFind,

        /// <summary>
        /// 从‘{0}’路径加载流异常，信息：‘{1}’。
        /// </summary>
        [Description("从‘{0}’路径加载流异常，信息：‘{1}’。")]
        LoadStreamFail,

        /// <summary>
        /// 数据处理适配器为空，可能客户端已掉线。
        /// </summary>
        [Description("数据处理适配器为空，可能客户端已掉线。")]
        NullDataAdapter,

        /// <summary>
        /// 客户端没有连接
        /// </summary>
        [Description("客户端没有连接。")]
        NotConnected
    }
}