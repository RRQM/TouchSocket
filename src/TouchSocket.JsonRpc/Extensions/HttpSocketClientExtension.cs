using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpSocketClientExtension
    /// </summary>
    public static class HttpSocketClientExtension
    {
        /// <summary>
        /// 标识是否为JsonRpc
        /// </summary>
        public static readonly DependencyProperty<bool> JsonRpcProperty =
            DependencyProperty<bool>.Register("JsonRpc", false);

        /// <summary>
        /// 获取<see cref="JsonRpcProperty"/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetJsonRpc(this IHttpSocketClient socketClient, bool value = true)
        {
            return socketClient.GetValue(JsonRpcProperty);
        }

        /// <summary>
        /// 设置<see cref="JsonRpcProperty"/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="value"></param>
        public static void SetJsonRpc(this IHttpSocketClient socketClient, bool value = true)
        {
            socketClient.SetValue(JsonRpcProperty, value);
        }
    }
}