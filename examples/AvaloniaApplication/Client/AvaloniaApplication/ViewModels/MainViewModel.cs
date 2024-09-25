using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc.Generators;
using TouchSocket.Sockets;

namespace AvaloniaApplication.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public MainViewModel()
    {
        this.LoginCommand = new RelayCommand(this.Login);
    }

    private string message;

    public string Message
    {
        get { return message; }
        set { SetProperty(ref message, value); }
    }


    private async void Login()
    {
        try
        {
            var websocketDmtpClient = new WebSocketDmtpClient();
            websocketDmtpClient.Setup(new TouchSocketConfig()
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetRemoteIPHost("ws://localhost:5043/WebSocketDmtp"));
            await websocketDmtpClient.ConnectAsync();

            this.Message = "login success";

            await Task.Delay(1000);

            var b = await websocketDmtpClient.GetDmtpRpcActor().LoginAsync("account", "pwd");

            this.Message = $"login result={b}";
        }
        catch (Exception ex)
        {
            this.Message = ex.Message;
        }

    }

    public RelayCommand LoginCommand { get; set; }
}
