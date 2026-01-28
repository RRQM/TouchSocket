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

using System.Diagnostics;
using TouchSocket.Core;

namespace AotDynamicMethodConsoleApp;

internal class Program
{
    private const int DefaultPerformanceTestCount = 10_000_000;

    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();
        consoleAction.OnException += OnException;

        // 注册所有示例命令
        RegisterCommands(consoleAction);

        consoleAction.ShowAll();
        await consoleAction.RunCommandLineAsync();
    }

    /// <summary>
    /// 注册所有示例命令
    /// </summary>
    private static void RegisterCommands(ConsoleAction consoleAction)
    {
        consoleAction.Add("1", "简单调用", SimpleRun);
        consoleAction.Add("2", "性能测试", Performance);
        consoleAction.Add("3", "多参数调用", MultiParameters);
        consoleAction.Add("4", "自定义动态调用", CustomDynamicMethod);
        consoleAction.Add("5", "异步Task调用", TaskRun);
        consoleAction.Add("6", "异步Task<T>调用", TaskObjectRun);
    }

    /// <summary>
    /// 全局异常处理
    /// </summary>
    private static void OnException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"错误: {ex.Message}");
        Console.ResetColor();
    }

    /// <summary>
    /// 示例1: 简单的方法调用
    /// </summary>
    private static void SimpleRun()
    {
        Console.WriteLine("=== 简单调用示例 ===");

        #region 简单调用示例
        var method = new Method(typeof(MyClass), nameof(MyClass.Run));
        var instance = new MyClass();
        method.InvokeAsync(instance);
        #endregion

        Console.WriteLine("调用完成！");
    }

    /// <summary>
    /// 示例2: 性能测试
    /// </summary>
    private static void Performance()
    {
        Console.WriteLine("=== 性能测试 ===");
        Console.WriteLine($"将执行 {DefaultPerformanceTestCount:N0} 次方法调用...\n");

        #region 性能测试代码
        var myClass = new MyClass();
        var method = new Method(typeof(MyClass), nameof(MyClass.Performance));
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < 10_000_000; i++)
        {
            method.InvokeAsync(myClass);
        }

        stopwatch.Stop();
        Console.WriteLine($"总耗时: {stopwatch.ElapsedMilliseconds} ms");
        #endregion

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ 执行完成");
            Console.WriteLine($"  总耗时: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"  平均耗时: {(double)stopwatch.ElapsedMilliseconds / DefaultPerformanceTestCount:F6} ms/call");
            Console.WriteLine($"  吞吐量: {DefaultPerformanceTestCount / (stopwatch.ElapsedMilliseconds / 1000.0):N0} calls/sec");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ 性能测试失败: {ex.Message}");
            Console.WriteLine($"  已完成部分测试，耗时: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// 示例3: 多参数调用，包括 out 和 ref 参数
    /// </summary>
    private static void MultiParameters()
    {
        Console.WriteLine("=== 多参数调用示例 ===");

        #region out和ref参数调用
        var method = new Method(typeof(MyClass), nameof(MyClass.MultiParameters));
        var instance = new MyClass();

        var parameters = new object[] { "hello", 0, 200 };
        method.InvokeAsync(instance, parameters);

        Console.WriteLine($"out参数b={parameters[1]}"); // 输出: out参数b=10
        Console.WriteLine($"ref参数c={parameters[2]}"); // 输出: ref参数c=201
        #endregion

        Console.WriteLine($"调用前参数: a=\"{parameters[0]}\", b={parameters[1]}, c={parameters[2]}");

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"调用后参数: a=\"{parameters[0]}\", b={parameters[1]}, c={parameters[2]}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"调用失败: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// 示例4: 使用自定义特性的动态方法调用
    /// </summary>
    private static void CustomDynamicMethod()
    {
        Console.WriteLine("=== 自定义动态方法调用示例 ===");

        #region 自定义特性调用
        var method = new Method(typeof(MyClass), nameof(MyClass.CustomDynamicMethod));
        var instance = new MyClass();
        method.InvokeAsync(instance);
        #endregion

        try
        {
            Console.WriteLine("自定义动态方法调用成功！");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"调用失败: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// 示例5: 异步 Task 方法调用
    /// </summary>
    private static async Task TaskRun()
    {
        Console.WriteLine("=== 异步Task调用示例 ===");

        #region 异步Task调用
        var method = new Method(typeof(MyClass), nameof(MyClass.TaskRun));
        var instance = new MyClass();

        // 判断是否为异步方法
        if (method.IsAwaitable)
        {
            await method.InvokeAsync(instance);
        }
        #endregion

        Console.WriteLine($"方法是否可等待: {method.IsAwaitable}");
        Console.WriteLine($"返回类型: {method.ReturnKind}");

        try
        {
            if (method.IsAwaitable)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("异步调用完成！");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("该方法不是异步方法");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"异步调用失败: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// 示例6: 异步 Task<T> 方法调用（带返回值）
    /// </summary>
    private static async Task TaskObjectRun()
    {
        Console.WriteLine("=== 异步Task<T>调用示例 ===");

        #region 异步Task泛型调用
        var method = new Method(typeof(MyClass), nameof(MyClass.TaskObjectRun));
        var instance = new MyClass();

        if (method.ReturnKind == MethodReturnKind.AwaitableObject)
        {
            var result = await method.InvokeAsync(instance);
            Console.WriteLine($"返回值: {result}"); // 输出: 返回值: 10
        }
        #endregion

        Console.WriteLine($"方法返回类型: {method.ReturnKind}");
        Console.WriteLine($"实际返回值类型: {method.RealReturnType?.Name ?? "void"}");

        try
        {
            if (method.ReturnKind == MethodReturnKind.AwaitableObject)
            {
                var result2 = await method.InvokeAsync(instance);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"异步调用完成，返回值: {result2}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("该方法不返回异步对象");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"异步调用失败: {ex.Message}");
            Console.ResetColor();
        }
    }
}

/// <summary>
/// 测试类，包含各种类型的动态方法
/// </summary>
#region 标记方法示例
public class MyClass
{
    /// <summary>
    /// 简单的void方法
    /// </summary>
    [DynamicMethod]
    public void Run()
    {
        Console.WriteLine("Run 方法被调用");
    }
    #endregion

    /// <summary>
    /// 用于性能测试的空方法
    /// </summary>
    #region 性能测试声明
    [DynamicMethod]
    public void Performance()
    {
        // 空方法体，用于性能测试
    }
    #endregion

    /// <summary>
    /// 包含多种参数类型的方法：普通参数、out参数、ref参数
    /// </summary>
    #region out和ref参数声明
    [DynamicMethod]
    public void MultiParameters(string a, out int b, ref int c)
    {
        b = 10;
        c = c + 1;
        Console.WriteLine($"a={a}, b={b}, c={c}");
    }
    #endregion

    /// <summary>
    /// 使用自定义特性标记的方法
    /// </summary>
    [MyDynamicMethod]
    public void CustomDynamicMethod()
    {
        Console.WriteLine("CustomDynamicMethod 方法被调用（使用自定义特性）");
    }

    /// <summary>
    /// 异步Task方法（无返回值）
    /// </summary>
    #region 异步Task调用声明
    [DynamicMethod]
    public async Task TaskRun()
    {
        Console.WriteLine("开始执行...");
        await Task.Delay(100);
        Console.WriteLine("执行完成");
    }
    #endregion

    /// <summary>
    /// 异步Task<T>方法（有返回值）
    /// </summary>
    #region 异步Task泛型调用声明
    [DynamicMethod]
    public async Task<int> TaskObjectRun()
    {
        await Task.Delay(100);
        return 10;
    }
    #endregion
}

/// <summary>
/// 自定义的动态方法特性
/// </summary>
#region 自定义特性声明
[DynamicMethod]
[AttributeUsage(AttributeTargets.Method)]
public class MyDynamicMethodAttribute : Attribute
{
}
#endregion

#region 缓存Method实例推荐
/// <summary>
/// 推荐的缓存方式：将Method实例缓存为静态字段，避免重复创建
/// </summary>
public static class MethodCache
{
    public static readonly Method RunMethod = new Method(typeof(MyClass), nameof(MyClass.Run));

    // 使用示例：
    // MethodCache.RunMethod.InvokeAsync(instance);
}
#endregion