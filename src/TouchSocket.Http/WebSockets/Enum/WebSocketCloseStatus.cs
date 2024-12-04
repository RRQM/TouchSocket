﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TouchSocket.Http.WebSockets
//{
//    /// <summary>
//    /// 表示WebSocket连接关闭的状态码。
//    /// </summary>
//    public enum WebSocketCloseStatus
//    {
//        /// <summary>
//        /// (1000) 连接已关闭，请求已完成。
//        /// </summary>
//        NormalClosure = 1000,

//        /// <summary>
//        /// (1001) 表示某个端点即将移除。服务器或客户端将变得不可用。
//        /// </summary>
//        EndpointUnavailable = 1001,

//        /// <summary>
//        /// (1002) 客户端或服务器因协议错误终止连接。
//        /// </summary>
//        ProtocolError = 1002,

//        /// <summary>
//        /// (1003) 客户端或服务器因不能接受接收到的数据类型而终止连接。
//        /// </summary>
//        InvalidMessageType = 1003,

//        /// <summary>
//        /// 没有指定错误。
//        /// </summary>
//        Empty = 1005,

//        /// <summary>
//        /// (1007) 客户端或服务器因接收到与消息类型不一致的数据而终止连接。
//        /// </summary>
//        InvalidPayloadData = 1007,

//        /// <summary>
//        /// (1008) 因为某个端点接收到违反其策略的消息，连接将被关闭。
//        /// </summary>
//        PolicyViolation = 1008,

//        /// <summary>
//        /// (1009) 客户端或服务器因接收到的消息过大而终止连接。
//        /// </summary>
//        MessageTooBig = 1009,

//        /// <summary>
//        /// (1010) 客户端因期望服务器协商扩展而终止连接。
//        /// </summary>
//        MandatoryExtension = 1010,

//        /// <summary>
//        /// (1011) 由于服务器上的错误，连接将由服务器关闭。
//        /// </summary>
//        InternalServerError = 1011
//    }
//}
