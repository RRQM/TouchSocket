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
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpcUtility
    /// </summary>
    public class TouchRpcUtility
    {
        /// <summary>
        /// TouchRpc字符串
        /// </summary>
        public const string TouchRpc = "TOUCHRPC";

        /// <summary>
        /// TouchRpc
        /// </summary>
        public static Protocol TouchRpcProtocol { get; private set; } = new Protocol(TouchRpc);

        /// <summary>
        /// 传输分包
        /// </summary>
        public const int TransferPackage = 1024 * 512;

        #region 基本协议，0-99；

        /// <summary>
        /// 握手
        /// </summary>
        public const short P_0_Handshake_Request = 0;

        /// <summary>
        /// 重置ID
        /// </summary>
        public const short P_1_ResetID_Request = -1;

        /// <summary>
        /// 握手响应
        /// </summary>
        public const short P_1000_Handshake_Response = -1000;

        /// <summary>
        /// 重置ID响应
        /// </summary>
        public const short P_1001_ResetID_Response = -1001;

        /// <summary>
        /// Ping回应
        /// </summary>
        public const short P_1002_Ping_Response = -1002;

        /// <summary>
        /// Ping回应
        /// </summary>
        public const short P_1003_Ping2C_Response = -1003;

        /// <summary>
        /// Ping
        /// </summary>
        public const short P_2_Ping_Request = -2;

        /// <summary>
        /// Ping
        /// </summary>
        public const short P_3_Ping2C_Request = -3;

        #endregion 基本协议，0-99；

        #region 通道协议，100-199；

        /// <summary>
        /// 创建一个面向对方的通道
        /// </summary>
        public const short P_100_CreateChannel_Request = -100;

        /// <summary>
        /// 数据协议
        /// </summary>
        public const short P_101_DataOrder = -101;

        /// <summary>
        /// 完成指令
        /// </summary>
        public const short P_102_CompleteOrder = -102;

        /// <summary>
        /// 取消
        /// </summary>
        public const short P_103_CancelOrder = -103;

        /// <summary>
        /// 释放
        /// </summary>
        public const short P_104_DisposeOrder = -104;

        /// <summary>
        /// 持续
        /// </summary>
        public const short P_105_HoldOnOrder = -105;

        /// <summary>
        /// 队列
        /// </summary>
        public const short P_106_QueueChangedOrder = -106;

        /// <summary>
        /// 创建一个面向其他客户端的通道
        /// </summary>
        public const short P_110_CreateChannel_2C_Request = -110;

        /// <summary>
        /// 数据协议
        /// </summary>
        public const short P_111_DataOrder_2C = -111;

        /// <summary>
        /// 面向其他客户端的通道
        /// </summary>
        public const short P_1110_CreateChannel_2C_Response = -1110;

        /// <summary>
        /// 完成指令
        /// </summary>
        public const short P_112_CompleteOrder_2C = -112;

        /// <summary>
        /// 取消
        /// </summary>
        public const short P_113_CancelOrder_2C = -113;

        /// <summary>
        /// 释放
        /// </summary>
        public const short P_114_DisposeOrder_2C = -114;

        /// <summary>
        /// 持续
        /// </summary>
        public const short P_115_HoldOnOrder_2C = -115;

        /// <summary>
        /// 队列
        /// </summary>
        public const short P_116_QueueChangedOrder_2C = -116;

        /// <summary>
        /// 创建一个面向其他客户端的通道
        /// </summary>
        public const short P_120_CreateChannel_FC = -120;

        #endregion 通道协议，100-199；

        #region Rpc协议，200-299；

        /// <summary>
        /// 调用响应
        /// </summary>
        public const short P_1200_Invoke_Response = -1200;

        /// <summary>
        /// 调用ID客户端响应
        /// </summary>
        public const short P_1201_Invoke2C_Response = -1201;

        /// <summary>
        /// 调用
        /// </summary>
        public const short P_200_Invoke_Request = -200;

        /// <summary>
        /// 调用ID客户端
        /// </summary>
        public const short P_201_Invoke2C_Request = -201;

        /// <summary>
        /// 取消调用
        /// </summary>
        public const short P_204_CancelInvoke = -204;

        /// <summary>
        /// 取消调用
        /// </summary>
        public const short P_205_CancelInvoke2C = -205;

        #endregion Rpc协议，200-299；

        #region EventBus协议 300-399

        /// <summary>
        /// 获取所有事件响应
        /// </summary>
        public const short P_1300_GetAllEvents_Response = -1300;

        /// <summary>
        /// 发布事件响应
        /// </summary>
        public const short P_1301_PublishEvent_Response = -1301;

        /// <summary>
        /// 取消发布事件响应
        /// </summary>
        public const short P_1302_UnpublishEvent_Response = -1302;

        /// <summary>
        /// 订阅事件响应
        /// </summary>
        public const short P_1303_SubscribeEvent_Response = -1303;

        /// <summary>
        /// 触发事件响应
        /// </summary>
        public const short P_1305_RaiseEvent_Response = -1305;

        /// <summary>
        /// 获取所有事件
        /// </summary>
        public const short P_300_GetAllEvents_Request = -300;

        /// <summary>
        /// 发布事件
        /// </summary>
        public const short P_301_PublishEvent_Request = -301;

        /// <summary>
        /// 取消发布事件
        /// </summary>
        public const short P_302_UnpublishEvent_Request = -302;

        /// <summary>
        /// 订阅事件
        /// </summary>
        public const short P_303_SubscribeEvent_Request = -303;

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public const short P_304_UnsubscribeEvent = -304;

        /// <summary>
        /// 触发事件
        /// </summary>
        public const short P_305_RaiseEvent_Request = -305;

        /// <summary>
        /// 分发事件
        /// </summary>
        public const short P_306_TriggerEvent = -306;

        #endregion EventBus协议 300-399

        #region Stream协议 400-499

        /// <summary>
        /// 向服务器发送流响应
        /// </summary>
        public const short P_1400_SendStreamToSocketClient_Response = -1400;

        /// <summary>
        /// 向服务器发送流
        /// </summary>
        public const short P_400_SendStreamToSocketClient_Request = -400;

        /// <summary>
        /// 向客户端发送流
        /// </summary>
        public const short P_401_SendStreamToClient = -401;
        #endregion Stream协议 400-499

        #region FileTransfer协议 500-599

        /// <summary>
        /// 拉取文件响应
        /// </summary>
        public const short P_1500_PullFile_Response = -1500;

        /// <summary>
        /// 开始拉取文件响应
        /// </summary>
        public const short P_1501_BeginPullFile_Response = -1501;

        /// <summary>
        /// 推送文件响应
        /// </summary>
        public const short P_1502_PushFile_Response = -1502;

        /// <summary>
        /// 从客户端拉取文件响应
        /// </summary>
        public const short P_1503_PullFile2C_Response = -1503;

        /// <summary>
        /// 客户端拉取文件转发响应
        /// </summary>
        public const short P_1504_PullFileFC_Response = -1504;

        /// <summary>
        /// 开始从客户端拉取文件响应
        /// </summary>
        public const short P_1505_BeginPullFile2C_Response = -1505;

        /// <summary>
        /// 开始从客户端拉取文件转发响应
        /// </summary>
        public const short P_1506_BeginPullFileFC_Response = -1506;

        /// <summary>
        /// 推送到客户端响应
        /// </summary>
        public const short P_1507_PushFile2C_Response = -1507;

        /// <summary>
        /// 推送到客户端转发响应
        /// </summary>
        public const short P_1508_PushFileFC_Response = -1508;

        /// <summary>
        /// 拉取文件
        /// </summary>
        public const short P_500_PullFile_Request = -500;

        /// <summary>
        /// 开始拉取文件
        /// </summary>
        public const short P_501_BeginPullFile_Request = -501;

        /// <summary>
        /// 推送文件
        /// </summary>
        public const short P_502_PushFile_Request = -502;

        /// <summary>
        /// 从客户端拉取文件
        /// </summary>
        public const short P_503_PullFile2C_Request = -503;

        /// <summary>
        /// 客户端拉取文件转发
        /// </summary>
        public const short P_504_PullFileFC_Request = -504;

        /// <summary>
        /// 开始从客户端拉取文件
        /// </summary>
        public const short P_505_BeginPullFile2C_Request = -505;

        /// <summary>
        /// 开始从客户端拉取文件转发
        /// </summary>
        public const short P_506_BeginPullFileFC_Request = -506;

        /// <summary>
        /// 推送到客户端
        /// </summary>
        public const short P_507_PushFile2C_Request = -507;

        /// <summary>
        /// 推送到客户端转发
        /// </summary>
        public const short P_508_PushFileFC_Request = -508;

        /// <summary>
        /// 推送文件状态确认
        /// </summary>
        public const short P_509_PushFileAck_Request = -509;

        /// <summary>
        /// 远程访问
        /// </summary>
        public const short P_510_RemoteAccess_Request = -510;

        /// <summary>
        /// 远程访问响应
        /// </summary>
        public const short P_1510_RemoteAccess_Response = -1510;

        #endregion FileTransfer协议 500-599

        #region Test

        /// <summary>
        /// 测试路由
        /// </summary>
        public const short P_600_Route = -600;

        #endregion Test
    }
}