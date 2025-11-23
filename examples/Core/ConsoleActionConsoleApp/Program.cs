//------------------------------------------------------------------------------
//  此代码版权(除特别声明或在XREF结尾的命名空间的代码)归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置,则按MIT开源协议授权
//  CSDN博客:https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频:https://space.bilibili.com/94253567
//  Gitee源代码仓库:https://gitee.com/RRQM_Home
//  Github源代码仓库:https://github.com/RRQM
//  API首页:https://touchsocket.net/
//  交流QQ群:234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace ConsoleActionConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // 基础示例
        BasicUsageExample();

        // 运行命令行示例
        // await RunCommandLineExample();
    }

    #region 控制台行为基础用法
    /// <summary>
    /// 基础用法示例
    /// </summary>
    private static void BasicUsageExample()
    {
        var consoleAction = new ConsoleAction("h|help|?"); // 设置帮助命令
        consoleAction.OnException += ConsoleAction_OnException; // 订阅执行异常输出

        // 添加命令
        consoleAction.Add("start|s", "启动服务", StartService);
        consoleAction.Add("stop|st", "停止服务", StopService);
        consoleAction.Add("status|ss", "查看状态", ShowStatus);
        consoleAction.Add("clear|c", "清空控制台", ClearConsole);

        // 显示所有注册的命令
        consoleAction.ShowAll();

        // 模拟执行命令
        Console.WriteLine("\n执行命令示例:");
        consoleAction.RunAsync("start").Wait();
        consoleAction.RunAsync("status").Wait();
    }
    #endregion

    #region 控制台行为异常处理
    /// <summary>
    /// 异常处理
    /// </summary>
    private static void ConsoleAction_OnException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"发生错误: {ex.Message}");
        Console.ResetColor();
    }
    #endregion

    #region 控制台行为添加同步命令
    /// <summary>
    /// 添加同步命令示例
    /// </summary>
    private static void AddSyncCommandExample()
    {
        var consoleAction = new ConsoleAction();

        // 添加无参数的同步命令
        consoleAction.Add("test1", "测试命令1", () =>
        {
            Console.WriteLine("执行测试命令1");
        });

        // 多个命令别名
        consoleAction.Add("start|s|run", "启动服务", StartService);
    }
    #endregion

    #region 控制台行为添加异步命令
    /// <summary>
    /// 添加异步命令示例
    /// </summary>
    private static void AddAsyncCommandExample()
    {
        var consoleAction = new ConsoleAction();

        // 添加异步命令
        consoleAction.Add("async-task", "异步任务", async () =>
        {
            Console.WriteLine("开始异步任务...");
            await Task.Delay(1000);
            Console.WriteLine("异步任务完成");
        });
    }
    #endregion

    #region 控制台行为运行命令行
    /// <summary>
    /// 运行命令行循环
    /// </summary>
    private static async Task RunCommandLineExample()
    {
        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;

        // 注册命令
        consoleAction.Add("start", "启动服务", StartService);
        consoleAction.Add("stop", "停止服务", StopService);
        consoleAction.Add("exit|quit", "退出程序", () =>
        {
            Console.WriteLine("程序退出");
            Environment.Exit(0);
        });

        // 显示所有命令
        consoleAction.ShowAll();

        // 运行命令行循环
        await consoleAction.RunCommandLineAsync();
    }
    #endregion

    #region 控制台行为获取所有命令
    /// <summary>
    /// 获取所有命令信息
    /// </summary>
    private static void GetAllCommandsExample()
    {
        var consoleAction = new ConsoleAction();
        consoleAction.Add("cmd1", "命令1", () => { });
        consoleAction.Add("cmd2|c2", "命令2", () => { });

        // 获取所有命令信息
        var allCommands = consoleAction.AllActionInfos;
        foreach (var cmd in allCommands)
        {
            Console.WriteLine($"命令: {cmd.FullOrder}, 描述: {cmd.Description}");
        }
    }
    #endregion

    // 示例命令实现方法
    private static void StartService()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 服务已启动");
        Console.ResetColor();
    }

    private static void StopService()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✓ 服务已停止");
        Console.ResetColor();
    }

    private static void ShowStatus()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("当前状态: 运行中");
        Console.WriteLine("启动时间: 2024-01-01 00:00:00");
        Console.WriteLine("活动连接: 0");
        Console.ResetColor();
    }

    private static void ClearConsole()
    {
        Console.Clear();
        Console.WriteLine("控制台已清空");
    }
}
