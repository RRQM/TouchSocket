//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpcParser配置
    /// </summary>
    public class JsonRpcParserConfig : TcpServiceConfig
    {
        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return (JsonRpcProtocolType)GetValue(ProtocolTypeProperty); }
            set { SetValue(ProtocolTypeProperty, value); }
        }

        /// <summary>
        /// 协议类型，
        /// 所需类型<see cref="JsonRpcProtocolType"/>
        /// </summary>
        public static readonly DependencyProperty ProtocolTypeProperty =
        DependencyProperty.Register("ProtocolType", typeof(JsonRpcProtocolType), typeof(JsonRpcParserConfig), JsonRpcProtocolType.Tcp);


        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public int MaxPackageSize
        {
            get { return (int)GetValue(MaxPackageSizeProperty); }
            set { SetValue(MaxPackageSizeProperty, value); }
        }

        /// <summary>
        /// 最大数据包长度，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty MaxPackageSizeProperty =
            DependencyProperty.Register("MaxPackageSize", typeof(int), typeof(JsonRpcParserConfig), 1024);



        /// <summary>
        /// 调用类型
        /// </summary>
        public InvokeType InvokeType
        {
            get { return (InvokeType)GetValue(InvokeTypeProperty); }
            set { SetValue(InvokeTypeProperty, value); }
        }

        /// <summary>
        /// 调用类型，所需类型<see cref="RRQMSocket.RPC.InvokeType"/>
        /// </summary>
        public static readonly DependencyProperty InvokeTypeProperty =
            DependencyProperty.Register("InvokeType", typeof(InvokeType), typeof(JsonRpcParserConfig), InvokeType.GlobalInstance);


    }
}