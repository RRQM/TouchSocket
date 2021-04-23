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
    /*
    若汝棋茗
    */

    /// <summary>
    /// 绑定参数
    /// </summary>
    public class BindSetting
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BindSetting()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="threadCount"></param>
        public BindSetting(string ip, int port, int threadCount)
        {
            this.IP = ip;
            this.Port = port;
            this.MultithreadThreadCount = threadCount;
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        private int multithreadThreadCount = 1;

        /// <summary>
        /// 多线程数量，最小值为1
        /// </summary>
        public int MultithreadThreadCount
        {
            get { return multithreadThreadCount; }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                multithreadThreadCount = value;
            }
        }
    }
}