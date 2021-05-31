using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// Tcp服务配置
    /// </summary>
    public class TcpServerConfig : ServerConfig
    {
        /// <summary>
        /// 挂起连接队列的最大长度。默认为30
        /// </summary>
        public int Backlog
        {
            get { return (int)GetValue(BacklogProperty); }
            set { SetValue(BacklogProperty, value); }
        }
        /// <summary>
        /// 挂起连接队列的最大长度
        /// </summary>
        public static readonly DependencyProperty BacklogProperty =
            DependencyProperty.Register("Backlog", typeof(int), typeof(TcpServerConfig), 30);

        

        /// <summary>
        /// 最大可连接数，默认为10000
        /// </summary>
        public int MaxCount
        {
            get { return (int)GetValue(MaxCountProperty); }
            set { SetValue(MaxCountProperty, value); }
        }


        /// <summary>
        /// 最大可连接数，默认为10000
        /// </summary>
        public static readonly DependencyProperty MaxCountProperty =
            DependencyProperty.Register("MaxCount", typeof(int), typeof(TcpServerConfig), 10000);


        /// <summary>
        /// 获取或设置分配ID的格式，
        /// 格式必须符合字符串格式，至少包含一个补位，
        /// 默认为“{0}-TCP”
        /// </summary>
        public string IDFormat
        {
            get { return (string)GetValue(IDFormatProperty); }
            set { SetValue(IDFormatProperty, value); }
        }

        /// <summary>
        /// 获取或设置分配ID的格式，
        /// 格式必须符合字符串格式，至少包含一个补位，
        /// 默认为“{0}-TCP”
        /// </summary>
        public static readonly DependencyProperty IDFormatProperty =
            DependencyProperty.Register("IDFormat", typeof(string), typeof(TcpServerConfig), "{0}-TCP");


        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval
        {
            get { return (int)GetValue(ClearIntervalProperty); }
            set { SetValue(ClearIntervalProperty, value); }
        }

        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public static readonly DependencyProperty ClearIntervalProperty =
            DependencyProperty.Register("ClearInterval", typeof(int), typeof(TcpServerConfig), 60);
    }
}
