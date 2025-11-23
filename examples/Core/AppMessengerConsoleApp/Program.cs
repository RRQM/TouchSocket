//------------------------------------------------------------------------------
//  此代码版权(除特别声明或在XREF结尾的命名空间的代码)归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置,则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace AppMessengerConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await RegisterInstanceExample();
        await RegisterStaticExample();
        await SendAsyncExample();
        await UnregisterExample();
        await AllowMultipleExample();

        Console.WriteLine("所有示例执行完成。");
        Console.ReadKey();
    }

    #region AppMessenger注册实例示例
    private static async Task RegisterInstanceExample()
    {
        Console.WriteLine("=== 注册实例示例 ===");
        
        var messageObject = new MessageObject();
        
        int add = await AppMessenger.Default.SendAsync<int>("Add", 20, 10);
        Console.WriteLine($"Add结果: {add}");

        int sub = await AppMessenger.Default.SendAsync<int>("Sub", 20, 10);
        Console.WriteLine($"Sub结果: {sub}");
        
        Console.WriteLine();
    }
    #endregion

    #region AppMessenger注册静态方法示例
    private static async Task RegisterStaticExample()
    {
        Console.WriteLine("=== 注册静态方法示例 ===");
        
        AppMessenger.Default.RegisterStatic<StaticMessageObject>();
        
        int multiply = await AppMessenger.Default.SendAsync<int>("StaticMultiply", 5, 4);
        Console.WriteLine($"StaticMultiply结果: {multiply}");

        Console.WriteLine();
    }
    #endregion

    #region AppMessenger触发示例
    private static async Task SendAsyncExample()
    {
        Console.WriteLine("=== 触发消息示例 ===");
        
        // 触发有返回值的消息
        int result = await AppMessenger.Default.SendAsync<int>("Add", 100, 50);
        Console.WriteLine($"Add(100, 50) = {result}");
        
        // 触发无返回值的消息
        await AppMessenger.Default.SendAsync("Notify", "这是一条通知消息");
        
        Console.WriteLine();
    }
    #endregion

    #region AppMessenger注销示例
    private static async Task UnregisterExample()
    {
        Console.WriteLine("=== 注销示例 ===");
        
        var tempObject = new TempMessageObject();
        
        // 注销前可以调用
        await AppMessenger.Default.SendAsync("TempMethod");
        Console.WriteLine("注销前调用成功");
        
        // 注销对象
        AppMessenger.Default.Unregister(tempObject);
        
        try
        {
            // 注销后无法调用
            await AppMessenger.Default.SendAsync("TempMethod");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"注销后调用失败: {ex.Message}");
        }
        
        Console.WriteLine();
    }
    #endregion

    #region AppMessenger允许多个订阅示例
    private static async Task AllowMultipleExample()
    {
        Console.WriteLine("=== 允许多个订阅示例 ===");
        
        // 创建新的AppMessenger实例并允许多个订阅
        var messenger = new AppMessenger { AllowMultiple = true };
        
        var subscriber1 = new MultiSubscriber("订阅者1");
        var subscriber2 = new MultiSubscriber("订阅者2");
        
        messenger.Register(subscriber1);
        messenger.Register(subscriber2);
        
        // 广播消息,所有订阅者都会收到
        await messenger.SendAsync("OnEvent", "测试事件");
        
        Console.WriteLine();
    }
    #endregion
}

#region AppMessenger实例类定义
public class MessageObject : IMessageObject
{
    public MessageObject()
    {
        AppMessenger.Default.Register(this);
    }

    [AppMessage]
    public Task<int> Add(int a, int b)
    {
        return Task.FromResult(a + b);
    }

    [AppMessage]
    public Task<int> Sub(int a, int b)
    {
        return Task.FromResult(a - b);
    }

    [AppMessage]
    public async Task Notify(string message)
    {
        await Task.CompletedTask;
        Console.WriteLine($"收到通知: {message}");
    }
}
#endregion

#region AppMessenger静态方法定义
public class StaticMessageObject : IMessageObject
{
    [AppMessage]
    public static Task<int> StaticMultiply(int a, int b)
    {
        return Task.FromResult(a * b);
    }
}
#endregion

#region AppMessenger临时对象定义
public class TempMessageObject : IMessageObject
{
    public TempMessageObject()
    {
        AppMessenger.Default.Register(this);
    }

    [AppMessage]
    public async Task TempMethod()
    {
        await Task.CompletedTask;
        Console.WriteLine("临时方法被调用");
    }
}
#endregion

#region AppMessenger多订阅者定义
public class MultiSubscriber : IMessageObject
{
    private readonly string _name;

    public MultiSubscriber(string name)
    {
        _name = name;
    }

    [AppMessage]
    public async Task OnEvent(string message)
    {
        await Task.CompletedTask;
        Console.WriteLine($"{_name} 收到消息: {message}");
    }
}
#endregion
