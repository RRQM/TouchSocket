// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Text;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace RpcPerformanceConsoleApp
{
    public partial class TestController : SingletonRpcServer
    {

        [DmtpRpc(MethodInvoke = true)]
        public int Sum(int a, int b) => a + b;

        [DmtpRpc(MethodInvoke = true)]
        public byte[] GetBytes(int length)
        {
            return new byte[length];
        }

        [DmtpRpc(MethodInvoke = true)]
        public string GetBigString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return stringBuilder.ToString();
        }
    }
}
