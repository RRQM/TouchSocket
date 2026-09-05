using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Ssh;

namespace SshConsoleApp;

internal class Program
{
    private static void Main()
    {
    }

    #region Ssh服务端创建服务端

    private static async Task CreateSshServiceAsync()
    {
        var service = new SshService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(7789)
            .SetSshServiceOption(new SshServiceOption
            {
                UserName = "touchsocket",
                Password = "123456",
                CommandExecutor = (command, _) => Task.FromResult(new SshCommandResult
                {
                    Command = Encoding.UTF8.GetBytes(command),
                    StandardOutput = Encoding.UTF8.GetBytes($"已执行命令：{command}"),
                    ExitStatus = 0
                })
            }));

        await service.StartAsync();
    }

    #endregion Ssh服务端创建服务端

    #region Ssh客户端连接并执行命令

    private static async Task ConnectAndExecuteCommandAsync()
    {
        var client = new SshClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .SetSshClientOption(new SshClientOption
            {
                UserName = "touchsocket",
                Password = "123456"
            }));

        await client.ConnectAsync();
        var result = await client.ExecuteCommandAsync("echo TouchSocket");
        Console.WriteLine(Encoding.UTF8.GetString(result.StandardOutput.Span));
        await client.CloseAsync("客户端主动关闭");
    }

    #endregion Ssh客户端连接并执行命令

    #region Ssh客户端发送保活请求

    private static async Task PingAsync(SshClient client)
    {
        var result = await client.PingAsync();
        Console.WriteLine(result.IsSuccess ? "SSH连接正常" : result.Message);
    }

    #endregion Ssh客户端发送保活请求

    #region Ssh客户端执行远程命令

    private static async Task ExecuteCommandAsync(SshClient client)
    {
        var result = await client.ExecuteCommandAsync("echo TouchSocket");
        Console.WriteLine(Encoding.UTF8.GetString(result.StandardOutput.Span));
        Console.WriteLine(Encoding.UTF8.GetString(result.StandardError.Span));
        Console.WriteLine($"退出状态：{result.ExitStatus}");
    }

    #endregion Ssh客户端执行远程命令

    #region Ssh客户端使用交互式Shell

    private static async Task UseShellAsync(SshClient client)
    {
        var shellResult = await client.OpenShellAsync(new SshShellOption
        {
            TerminalType = "xterm-256color",
            TerminalWidth = 120,
            TerminalHeight = 40
        });
        if (!shellResult.IsSuccess)
        {
            Console.WriteLine(shellResult.Message);
            return;
        }

        using var shell = shellResult.Value;
        await shell.WriteAsync(Encoding.UTF8.GetBytes("echo TouchSocket\n"));
        var buffer = new byte[1024];
        var readResult = await shell.ReadAsync(buffer);
        if (readResult.IsSuccess)
        {
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, readResult.Value));
        }

        await shell.CloseAsync();
    }

    #endregion Ssh客户端使用交互式Shell

    #region Ssh客户端上传文件

    private static async Task UploadFileAsync(SshClient client)
    {
        await using var stream = File.OpenRead("local.txt");
        var result = await client.UploadFileAsync(new SshFileOperator
        {
            Stream = stream,
            Path = "/tmp/remote.txt",
            Overwrite = true,
            Resume = true,
            MaxSpeed = 1024 * 1024,
            ProgressChanged = transferred => Console.WriteLine($"已上传：{transferred} 字节")
        });
        Console.WriteLine(result.IsSuccess ? "上传完成" : result.Message);
    }

    #endregion Ssh客户端上传文件

    #region Ssh客户端下载文件

    private static async Task DownloadFileAsync(SshClient client)
    {
        await using var stream = File.Create("download.txt");
        var result = await client.DownloadFileAsync(new SshFileOperator
        {
            Stream = stream,
            Path = "/tmp/remote.txt",
            Overwrite = true,
            Resume = true,
            MaxSpeed = 1024 * 1024,
            ProgressChanged = transferred => Console.WriteLine($"已下载：{transferred} 字节")
        });
        Console.WriteLine(result.IsSuccess ? "下载完成" : result.Message);
    }

    #endregion Ssh客户端下载文件

    #region Ssh客户端列举远程目录

    private static async Task ListDirectoryAsync(SshClient client)
    {
        var result = await client.ListDirectoryAsync("/tmp");
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Message);
            return;
        }

        foreach (var file in result.Value)
        {
            Console.WriteLine(file.Name);
        }
    }

    #endregion Ssh客户端列举远程目录

    #region Ssh客户端检查远程路径

    private static async Task CheckRemotePathAsync(SshClient client)
    {
        var result = await client.ExistsAsync("/tmp/remote.txt");
        Console.WriteLine(result.IsSuccess && result.Value ? "远程路径存在" : "远程路径不存在或检查失败");
    }

    #endregion Ssh客户端检查远程路径

    #region Ssh客户端创建远程目录

    private static async Task CreateDirectoryAsync(SshClient client)
    {
        var result = await client.CreateDirectoryAsync("/tmp/touchsocket");
        Console.WriteLine(result.IsSuccess ? "目录创建完成" : result.Message);
    }

    #endregion Ssh客户端创建远程目录

    #region Ssh客户端删除远程文件

    private static async Task DeleteFileAsync(SshClient client)
    {
        var result = await client.DeleteFileAsync("/tmp/remote.txt");
        Console.WriteLine(result.IsSuccess ? "文件删除完成" : result.Message);
    }

    #endregion Ssh客户端删除远程文件

    #region Ssh客户端删除远程目录

    private static async Task DeleteDirectoryAsync(SshClient client)
    {
        var result = await client.DeleteDirectoryAsync("/tmp/touchsocket");
        Console.WriteLine(result.IsSuccess ? "目录删除完成" : result.Message);
    }

    #endregion Ssh客户端删除远程目录

    #region Ssh客户端重命名远程路径

    private static async Task RenameRemotePathAsync(SshClient client)
    {
        var result = await client.RenameAsync("/tmp/remote.txt", "/tmp/renamed.txt");
        Console.WriteLine(result.IsSuccess ? "路径重命名完成" : result.Message);
    }

    #endregion Ssh客户端重命名远程路径
}
