using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
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
        this.Login1wCommand = new RelayCommand(this.Login1w);
    }

    

    private string message;

    public string Message
    {
        get { return message; }
        set { SetProperty(ref message, value); }
    }

    private string account;

    public string Account
    {
        get { return account; }
        set { SetProperty(ref account, value); }
    }

    private string password;

    public string Password
    {
        get { return password; }
        set { SetProperty(ref password, value); }
    }


    private async void Login()
    {
        try
        {
            using var websocketDmtpClient = new WebSocketDmtpClient();
            await websocketDmtpClient.SetupAsync(new TouchSocketConfig()
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

            this.Log("login success");

            var b = await websocketDmtpClient.GetDmtpRpcActor().LoginAsync(this.account, this.password);

            this.Log($"login result={b}");
        }
        catch (Exception ex)
        {
            this.Message = ex.Message;
        }

    }

    private async void Login1w()
    {
        try
        {
            using var websocketDmtpClient = new WebSocketDmtpClient();
            await websocketDmtpClient.SetupAsync(new TouchSocketConfig()
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

            this.Log("login success");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 10000; i++)
            {
                var b = await websocketDmtpClient.GetDmtpRpcActor().LoginAsync(this.account, this.password);
                if (i%100==0)
                {
                    this.Log(i.ToString());
                }
            }
            
            stopwatch.Stop();

            this.Log($"login result={stopwatch.Elapsed}");
        }
        catch (Exception ex)
        {
            this.Message = ex.Message;
        }
    }

    private void Log(string msg)
    {
        this.Message = $"{msg}\r\n{this.message}";
    }

    public RelayCommand LoginCommand { get; set; }
    public RelayCommand Login1wCommand { get; set; }
}
