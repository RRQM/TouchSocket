//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器接口
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 绑定TCP服务
        /// </summary>
        /// <param name="setting"></param>
        void Bind(BindSetting setting);

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        bool IsBind { get; }

        /// <summary>
        /// 内存池实例
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 获取或设置最大可连接数
        /// </summary>
        int MaxCount { get; set;}

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活）
        /// </summary>
        bool IsCheckClientAlive { get; set; }

    }
}