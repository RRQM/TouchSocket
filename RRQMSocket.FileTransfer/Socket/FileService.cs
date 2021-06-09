//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.Reflection;
using RRQMCore.ByteManager;
using RRQMCore.Serialization;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public class FileService : TokenService<FileSocketClient>, IFileService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileService()
        {
            this.BufferLength = 1024 * 64;
            this.operationMap = new OperationMap();
        }

        #region 属性

        /// <summary>
        /// 获取下载速度
        /// </summary>
        public long DownloadSpeed
        {
            get
            {
                this.downloadSpeed = Speed.downloadSpeed;
                Speed.downloadSpeed = 0;
                return this.downloadSpeed;
            }
        }

        /// <summary>
        /// 获取上传速度
        /// </summary>
        public long UploadSpeed
        {
            get
            {
                this.uploadSpeed = Speed.uploadSpeed;
                Speed.uploadSpeed = 0;
                return this.uploadSpeed;
            }
        }


        private bool breakpointResume;
        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public bool BreakpointResume
        {
            get { return breakpointResume; }
        }

        private long maxDownloadSpeed;
        /// <summary>
        /// 最大下载速度
        /// </summary>
        public long MaxDownloadSpeed
        {
            get { return maxDownloadSpeed; }
            set { maxUploadSpeed = value; }
        }

        private long maxUploadSpeed;
        /// <summary>
        /// 最大上传速度
        /// </summary>
        public long MaxUploadSpeed
        {
            get { return maxUploadSpeed; }
            set { maxUploadSpeed = value; }
        }
        #endregion 属性

        #region 字段

        private long downloadSpeed;
        private long uploadSpeed;
        private OperationMap operationMap;
        #endregion 字段

        #region 事件

        /// <summary>
        /// 传输文件之前
        /// </summary>
        public event RRQMFileOperationEventHandler BeforeFileTransfer;

        /// <summary>
        /// 当文件传输完成时
        /// </summary>
        public event RRQMTransferFileMessageEventHandler FinishedFileTransfer;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        public event RRQMReturnBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 收到字节
        /// </summary>
        public event RRQMBytesEventHandler Received;
        #endregion 事件

        /// <summary>
        /// 注册操作
        /// </summary>
        /// <param name="operation"></param>
        public void RegisterOperation(IOperation operation)
        {
            MethodInfo[] methods = operation.GetType().GetMethods();
            foreach (var mathod in methods)
            {
                OperationAttribute attribute = mathod.GetCustomAttribute<OperationAttribute>();
                if (attribute != null)
                {
                    string key = string.IsNullOrEmpty(attribute.OperationName) ? mathod.Name : attribute.OperationName;
                    OperationEntity entity = new OperationEntity();
                    entity.instance = operation;
                    entity.methodInfo = mathod;
                    operationMap.Add(key, entity);
                }
            }
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected override void LoadConfig(ServerConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.breakpointResume = (bool)serverConfig.GetValue(FileServiceConfig.BreakpointResumeProperty);
            this.maxDownloadSpeed = (long)serverConfig.GetValue(FileServiceConfig.MaxDownloadSpeedProperty);
            this.maxUploadSpeed = (long)serverConfig.GetValue(FileServiceConfig.MaxUploadSpeedProperty);
        }

        /// <summary>
        /// 创建完成FileSocketClient
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="creatOption"></param>
        protected sealed override void OnCreateSocketCliect(FileSocketClient tcpSocketClient, CreateOption creatOption)
        {
            tcpSocketClient.breakpointResume = this.BreakpointResume;
            tcpSocketClient.MaxDownloadSpeed = this.MaxDownloadSpeed;
            tcpSocketClient.MaxUploadSpeed = this.MaxUploadSpeed;
            if (creatOption.NewCreate)
            {
                tcpSocketClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
                tcpSocketClient.BeforeFileTransfer = this.OnBeforeFileTransfer;
                tcpSocketClient.FinishedFileTransfer = this.OnFinishedFileTransfer;
                tcpSocketClient.ReceivedBytesThenReturn = this.OnReceivedBytesThenReturn;
                tcpSocketClient.Received = this.OnReceivedBytes;
                tcpSocketClient.CallOperation = this.OnCallOperation;
            }
            tcpSocketClient.AgreementHelper = new RRQMAgreementHelper(tcpSocketClient);
        }

        private void OnBeforeFileTransfer(object sender, FileOperationEventArgs e)
        {
            this.BeforeFileTransfer?.Invoke(sender, e);
        }

        private void OnFinishedFileTransfer(object sender, TransferFileMessageArgs e)
        {
            this.FinishedFileTransfer?.Invoke(sender, e);
        }

        private void OnReceivedBytesThenReturn(object sender, ReturnBytesEventArgs e)
        {
            this.ReceivedBytesThenReturn?.Invoke(sender, e);
        }

        private void OnReceivedBytes(object sender, BytesEventArgs e)
        {
            this.Received?.Invoke(sender, e);
        }

        private void OnCallOperation(FileSocketClient sender, ByteBlock  byteBlock,ByteBlock returnBlock)
        {
            OperationContext context = OperationContext.Deserialize(byteBlock.Buffer, 4);
            if (this.operationMap.TryGet(context.OperationName, out OperationEntity entity))
            {
                try
                {
                    ParameterInfo[] parameters = entity.methodInfo.GetParameters();
                    object[] ps = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ps[i] = SerializeConvert.RRQMBinaryDeserialize(context.ParametersBytes[i], 0, parameters[i].ParameterType);
                    }

                    object returnObj = entity.methodInfo.Invoke(entity.instance, ps);
                    context.ReturnParameterBytes = SerializeConvert.RRQMBinarySerialize(returnObj, true);
                    context.Status = 1;
                }
                catch (System.Exception ex)
                {
                    context.ParametersBytes = null;
                    context.ReturnParameterBytes = null;
                    context.Status = 2;
                    context.Message = ex.Message;
                }
                
            }
            else
            {
                context.ParametersBytes = null;
                context.ReturnParameterBytes = null;
                context.Status = 2;
                context.Message = "无此操作";
            }
            context.Serialize(returnBlock);
        }
    }
}