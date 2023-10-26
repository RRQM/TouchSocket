using TouchSocket.Core;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IJsonRpcClientBase
    /// </summary>
    public interface IJsonRpcActionClient : IJsonRpcClient
    {
        /// <summary>
        /// WaitHandle
        /// </summary>
        WaitHandlePool<JsonRpcWaitResult> WaitHandle { get; }

        /// <summary>
        /// 收到JsonRpc的响应数据
        /// </summary>
        /// <param name="jsonString"></param>
        void InputResponseString(string jsonString);
    }
}