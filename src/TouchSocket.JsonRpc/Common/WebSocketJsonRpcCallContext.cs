namespace TouchSocket.JsonRpc
{
    internal class WebSocketJsonRpcCallContext : JsonRpcCallContextBase
    {
        public WebSocketJsonRpcCallContext(object caller, string jsonString) : base(caller, jsonString)
        {
        }
    }
}