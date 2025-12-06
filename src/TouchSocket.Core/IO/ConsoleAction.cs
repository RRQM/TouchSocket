//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 控制台行为
/// </summary>
public sealed class ConsoleAction
{
    private readonly Dictionary<string, ConsoleActionInfo> m_actions = new Dictionary<string, ConsoleActionInfo>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="helpOrder">帮助信息指令，如："h|help|?"</param>
    public ConsoleAction(string helpOrder = "h|help|?")
    {
        this.HelpOrder = helpOrder;

        this.Add(helpOrder, "帮助信息", this.ShowAll);

        this.ShowBanner();
    }

    /// <summary>
    /// 执行异常
    /// </summary>
    public event Action<Exception> OnException;

    /// <summary>
    /// 帮助信息指令
    /// </summary>
    public string HelpOrder { get; }

    /// <summary>
    /// 所有命令信息
    /// </summary>
    public IReadOnlyList<ConsoleActionInfo> AllActionInfos => this.m_actions.Values.Where(a => a.FullOrder != this.HelpOrder).ToList();

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="order">指令，多个指令用"|"分割</param>
    /// <param name="description">描述</param>
    /// <param name="action"></param>
    public void Add(string order, string description, Action action)
    {
        Task Run()
        {
            action.Invoke();
            return EasyTask.CompletedTask;
        }
        this.Add(order, description, Run);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="order">指令，多个指令用"|"分割</param>
    /// <param name="description">描述</param>
    /// <param name="action"></param>
    public void Add(string order, string description, Func<Task> action)
    {
        var orders = order.ToLower().Split('|');
        foreach (var item in orders)
        {
            this.m_actions.Add(item, new ConsoleActionInfo(description, order, action));
        }
    }

    /// <summary>
    /// 执行，返回值仅表示是否有这个指令，异常获取请使用<see cref="OnException"/>
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public async Task<bool> RunAsync(string order)
    {
        if (this.m_actions.TryGetValue(order.ToLower(), out var vAction))
        {
            try
            {
                await vAction.Action.Invoke().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.WriteError($"执行命令时发生错误: {ex.Message}");
                OnException?.Invoke(ex);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 运行
    /// </summary>
    public async Task RunCommandLineAsync()
    {
        while (true)
        {
            this.WritePrompt("请输入命令: ");
            var str = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(str))
            {
                continue;
            }

            if (!await this.RunAsync(str).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                this.WriteWarning($"没有找到命令 '{str}'，输入 '{this.HelpOrder.Split('|')[0]}' 查看帮助信息。");
            }
        }
    }

    /// <summary>
    /// 显示所有注册指令
    /// </summary>
    public void ShowAll()
    {
        this.WriteTitle("\n可用命令列表:");
        this.WriteLine();

        var maxOrderLength = this.m_actions.Values.Max(a => a.FullOrder.Length);
        var separator = new string('─', maxOrderLength + 4);

        var distinctActions = new List<string>();
        foreach (var item in this.m_actions)
        {
            if (!distinctActions.Contains(item.Value.FullOrder.ToLower()))
            {
                distinctActions.Add(item.Value.FullOrder.ToLower());

                this.WriteCommand($"  [{item.Value.FullOrder}]");
                var padding = maxOrderLength - item.Value.FullOrder.Length + 2;
                Console.Write(new string(' ', padding));
                this.WriteDescription(item.Value.Description);
                this.WriteLine();
            }
        }

        this.WriteLine();
    }

    /// <summary>
    /// 显示Banner
    /// </summary>
    private void ShowBanner()
    {
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Cyan;
        var title = @"

  _______                   _       _____               _          _
 |__   __|                 | |     / ____|             | |        | |
    | |  ___   _   _   ___ | |__  | (___    ___    ___ | | __ ___ | |_
    | | / _ \ | | | | / __|| '_ \  \___ \  / _ \  / __|| |/ // _ \| __|
    | || (_) || |_| || (__ | | | | ____) || (_) || (__ |   <|  __/| |_
    |_| \___/  \__,_| \___||_| |_||_____/  \___/  \___||_|\_\\___| \__|

";
        Console.WriteLine(title);

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" " + new string('─', 67));

        this.WriteInfo("     Author     : ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("若汝棋茗");

        this.WriteInfo("     Version    : ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

        this.WriteInfo("     Gitee      : ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("https://gitee.com/rrqm_home");

        this.WriteInfo("     Github     : ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("https://github.com/rrqm");

        this.WriteInfo("     API        : ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("https://touchsocket.net/");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" " + new string('─', 67));

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }

    /// <summary>
    /// 写入标题
    /// </summary>
    private void WriteTitle(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入命令
    /// </summary>
    private void WriteCommand(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入描述
    /// </summary>
    private void WriteDescription(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入提示
    /// </summary>
    private void WritePrompt(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入警告
    /// </summary>
    private void WriteWarning(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入错误
    /// </summary>
    private void WriteError(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入信息
    /// </summary>
    private void WriteInfo(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入空行
    /// </summary>
    private void WriteLine()
    {
        Console.WriteLine();
    }
}