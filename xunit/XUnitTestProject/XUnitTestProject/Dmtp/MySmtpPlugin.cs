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
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Dmtp.RouterPackage;
using XUnitTestProject.Dmtp._Packages;

namespace XUnitTestProject.Dmtp
{
    internal class MyDmtpPlugin : PluginBase, IDmtpReceivedPlugin, IDmtpRoutingPlugin, IDmtpFileTransferingPlugin, IDmtpRouterPackagePlugin
    {
        public Task OnCreateChannelAsync(IDmtpActorObject client, CreateChannelEventArgs e)
        {
            return Task.CompletedTask;
        }

        public async Task OnLoadingStream(IDmtpActorObject client, LoadingStreamEventArgs e)
        {
            if (e.Metadata?["1"] == "1")
            {
                e.IsPermitOperation = true;
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(10));
                }
            }
            else if (e.Metadata?["2"] == "2")
            {
                e.IsPermitOperation = true;
                e.DataCompressor = new GZipDataCompressor();
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(10));
                }
            }
            else
            {
                e.IsPermitOperation = false;
                e.Message = "fail";
            }

            await e.InvokeNext();
        }

        public async Task OnReceivedRouterPackage(IDmtpActorObject client, RouterPackageEventArgs e)
        {
            var testPackage = new TestPackage()
            {
                Message = "message",
                Status = 1
            };
            await e.ResponseAsync(testPackage);
            await e.InvokeNext();
        }

        Task IDmtpFileTransferingPlugin<IDmtpActorObject>.OnDmtpFileTransfering(IDmtpActorObject client, FileTransferingEventArgs e)
        {
            e.IsPermitOperation = true;
            e.Message = "123";

            return e.InvokeNext();
        }

        Task IDmtpReceivedPlugin<IDmtpActorObject>.OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            var protocol = e.DmtpMessage.ProtocolFlags;
            var byteBlock = e.DmtpMessage.BodyByteBlock;
            Console.WriteLine($"TcpDmtpService收到数据，协议为：{protocol}，数据长度为：{byteBlock.Len}");
            switch (protocol)
            {
                case 10000:
                    {
                        break;
                    }
                case 10001:
                    {
                        var id = byteBlock.ReadString();
                        Task.Run(() =>
                        {
                            try
                            {
                                Console.WriteLine($"oldID={client.DmtpActor.Id},newID={id}");
                                client.DmtpActor.ResetId(id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"重置ID错误，{ex.Message}");
                            }
                        });

                        break;
                    }
                case 20001:
                    {
                        break;
                    }
                default:
                    client.Send(protocol, byteBlock.ToArray(2));
                    break;
            }

            return e.InvokeNext();
        }

        Task IDmtpRoutingPlugin<IDmtpActorObject>.OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
        {
            Console.WriteLine(e.RouterType);
            e.IsPermitOperation = true;

            return e.InvokeNext();
        }
    }
}