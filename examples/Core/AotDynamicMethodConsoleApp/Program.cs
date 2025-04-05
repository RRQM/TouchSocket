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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace AotDynamicMethodConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;
        consoleAction.Add("1", "简单调用", SimpleRun);
        consoleAction.Add("2", "IL调用", ILRun);
        consoleAction.Add("3", "表达式树调用", ExpressionRun);
        consoleAction.Add("4", "反射调用", ReflectRun);
        consoleAction.Add("5", "源生成调用", SourceGeneratorRun);
        consoleAction.Add("6", "性能测试", Performance);
        consoleAction.Add("7", "多参数调用", MultiParameters);
        consoleAction.Add("8", "自定义动态调用", CustomDynamicMethod);
        consoleAction.Add("9", "TaskRun", TaskRun);
        consoleAction.Add("10", "TaskObjectRun", TaskObjectRun);

        consoleAction.ShowAll();
        await consoleAction.RunCommandLineAsync();
    }

    private static void ConsoleAction_OnException(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    static void SimpleRun()
    {
        Method method = new Method(typeof(MyClass), nameof(MyClass.Run));

        MyClass myClass = new MyClass();
        method.Invoke(myClass);
    }

    static void ILRun()
    {
        Method method = new Method(typeof(MyClass), nameof(MyClass.Run), DynamicBuilderType.IL);

        MyClass myClass = new MyClass();
        method.Invoke(myClass);
    }
    static void ExpressionRun()
    {
        Method method = new Method(typeof(MyClass), nameof(MyClass.Run), DynamicBuilderType.Expression);

        MyClass myClass = new MyClass();
        method.Invoke(myClass);
    }

    static void ReflectRun()
    {
        Method method = new Method(typeof(MyClass), nameof(MyClass.Run), DynamicBuilderType.Reflect);

        MyClass myClass = new MyClass();
        method.Invoke(myClass);
    }

    static void SourceGeneratorRun()
    {
        Method method = new Method(typeof(MyClass), nameof(MyClass.Run), DynamicBuilderType.SourceGenerator);

        MyClass myClass = new MyClass();
        method.Invoke(myClass);
    }

    static void Performance()
    {
        int count = 10000000;

        MyClass myClass = new MyClass();

        Stopwatch stopwatch = new Stopwatch();

        var methods = GetMethods(typeof(MyClass), nameof(MyClass.Performance));

        foreach (var item in methods)
        {
            stopwatch.Restart();
            try
            {
                var method = item;
                for (int i = 0; i < count; i++)
                {
                    method.Invoke(myClass);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Method BuilderType={item.DynamicBuilderType},Time={stopwatch.ElapsedMilliseconds}");
            }
        }

    }

    static void MultiParameters()
    {
        MyClass myClass = new MyClass();

        var methods = GetMethods(typeof(MyClass), nameof(MyClass.MultiParameters));

        foreach (var item in methods)
        {
            object[] ps = new object[] { "hello", 0, 200 };
            try
            {
                var method = item;
                method.Invoke(myClass, ps);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Method BuilderType={item.DynamicBuilderType},ps0={ps[0]},ps1={ps[1]},ps2={ps[2]}");
            }
        }

    }

    static void CustomDynamicMethod()
    {
        MyClass myClass = new MyClass();

        var methods = GetMethods(typeof(MyClass), nameof(MyClass.CustomDynamicMethod));

        foreach (var item in methods)
        {
            try
            {
                var method = item;
                method.Invoke(myClass);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Method BuilderType={item.DynamicBuilderType}");
            }
        }

    }

    static async Task TaskRun()
    {
        MyClass myClass = new MyClass();

        var methods = GetMethods(typeof(MyClass), nameof(MyClass.TaskRun));

        foreach (var item in methods)
        {
            try
            {
                var method = item;
                if (method.TaskType == TaskReturnType.Task)
                {
                    await method.InvokeAsync(myClass);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Method BuilderType={item.DynamicBuilderType}");
            }
        }

    }

    static async Task TaskObjectRun()
    {
        MyClass myClass = new MyClass();

        var methods = GetMethods(typeof(MyClass), nameof(MyClass.TaskObjectRun));

        foreach (var item in methods)
        {
            try
            {
                var method = item;
                if (method.TaskType == TaskReturnType.TaskObject)
                {
                    var result = await method.InvokeObjectAsync(myClass);
                    Console.WriteLine($"result={result}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Method BuilderType={item.DynamicBuilderType}");
            }
        }

    }

    static List<Method> GetMethods(Type type, string name)
    {
        var methods = new List<Method>();
        foreach (var item in Enum.GetValues(typeof(DynamicBuilderType)).OfType<DynamicBuilderType>())
        {
            try
            {
                methods.Add(new Method(type, name, item));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return methods;
    }

    static bool IsDynamicCodeCompiled()
    {
#if NET8_0_OR_GREATER
        return RuntimeFeature.IsDynamicCodeCompiled;
#else
        return true;
#endif
    }
}

public class MyClass
{
    [DynamicMethod]
    public void Run()
    {
        Console.WriteLine("Run");
    }

    [DynamicMethod]
    public void Performance()
    {

    }

    [DynamicMethod]
    public void MultiParameters(string a, out int b, ref int c)
    {
        b = 10;
        c = c + 1;
        Console.WriteLine("MultiParameters");
    }

    [MyDynamicMethod]
    public void CustomDynamicMethod()
    {
        Console.WriteLine("CustomDynamicMethod");
    }

    [DynamicMethod]
    public async Task TaskRun()
    {
        Console.WriteLine("TaskRun");
        await Task.CompletedTask;
    }

    [DynamicMethod]
    public async Task<int> TaskObjectRun()
    {
        Console.WriteLine("TaskObjectRun");
        await Task.CompletedTask;
        return 10;
    }
}

[DynamicMethod]
[AttributeUsage(AttributeTargets.Method)]
public class MyDynamicMethodAttribute : Attribute
{
}
