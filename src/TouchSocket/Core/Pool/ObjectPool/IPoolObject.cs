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

namespace TouchSocket.Core.Pool
{
    /// <summary>
    /// 对象池单位接口
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// 是否为新建对象
        /// </summary>
        bool NewCreate { get; set; }

        /// <summary>
        /// 初创建对象
        /// </summary>
        void Create();

        /// <summary>
        /// 重新创建对象
        /// </summary>
        void Recreate();

        /// <summary>
        /// 销毁对象
        /// </summary>
        void Destroy();
    }
}