//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类接口
    /// </summary>
    public interface ISocketClient : IClient
    {
        /// <summary>
        /// 用于索引的ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 标记
        /// </summary>
        object Flag { get; set; }

        /// <summary>
        /// 判断是否在线
        /// </summary>
        bool Online { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        DataHandlingAdapter DataHandlingAdapter { get; set; }

        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        IService Service { get; }
    }
}