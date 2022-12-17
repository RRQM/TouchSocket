using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpcServiceExtension
    /// </summary>
    public static class TouchRpcServiceExtension
    {
        /// <inheritdoc cref="ITouchRpcService.Send(string, short, byte[], int, int)"/>
        public static void Send(this ITouchRpcService service, string targetId, short protocol, byte[] buffer)
        {
            service.Send(targetId, protocol, buffer, 0, buffer.Length);
        }

        /// <inheritdoc cref="ITouchRpcService.Send(string, short, byte[], int, int)"/>
        public static void Send(this ITouchRpcService service, string targetId, short protocol)
        {
            service.Send(targetId, protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
        }

        /// <inheritdoc cref="ITouchRpcService.Send(string, short, byte[], int, int)"/>
        public static void Send(this ITouchRpcService service, string targetId, short protocol, ByteBlock byteBlock)
        {
            service.Send(targetId, protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <inheritdoc cref="ITouchRpcService.SendAsync(string, short, byte[], int, int)"/>
        public static Task SendAsync(this ITouchRpcService service, string targetId, short protocol, byte[] buffer)
        {
            return service.SendAsync(targetId, protocol, buffer, 0, buffer.Length);
        }

        /// <inheritdoc cref="ITouchRpcService.SendAsync(string, short, byte[], int, int)"/>
        public static Task SendAsync(this ITouchRpcService service, string targetId, short protocol)
        {
            return service.SendAsync(targetId, protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
        }
    }
}
