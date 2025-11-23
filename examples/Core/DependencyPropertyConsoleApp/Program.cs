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

using TouchSocket.Core;

namespace DependencyPropertyConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        // 演示内部声明
        InternalDeclarationExample();

        // 演示扩展声明
        ExtensionDeclarationExample();

        // 演示应用场景
        ScenarioExample();

        Console.ReadKey();
    }

    static void InternalDeclarationExample()
    {
        #region 依赖属性内部声明示例
        var obj = new MyDependencyObject();
        obj.MyProperty1 = 100;
        Console.WriteLine($"MyProperty1的值: {obj.MyProperty1}");
        #endregion
    }

    static void ExtensionDeclarationExample()
    {
        #region 依赖属性扩展声明示例
        var obj = new MyDependencyObject();
        
        // 使用扩展方法设置和获取
        obj.SetMyProperty2(200);
        var value = obj.GetMyProperty2();
        Console.WriteLine($"MyProperty2的值: {value}");

        // 也可以直接使用SetValue和GetValue
        obj.SetValue(DependencyExtensions.MyPropertyProperty2, 300);
        var value2 = obj.GetValue(DependencyExtensions.MyPropertyProperty2);
        Console.WriteLine($"MyProperty2的新值: {value2}");
        #endregion
    }

    static void ScenarioExample()
    {
        #region 依赖属性应用场景示例
        var person = new Person { Age = 25 };
        
        // 为Person对象动态添加Name属性
        person.SetName("张三");
        Console.WriteLine($"姓名: {person.GetName()}, 年龄: {person.Age}");

        // 为Person对象动态添加Address属性
        person.SetAddress("北京市");
        Console.WriteLine($"地址: {person.GetAddress()}");
        #endregion
    }
}

#region 依赖属性内部声明类定义
class MyDependencyObject : DependencyObject
{
    /// <summary>
    /// 属性项
    /// </summary>
    public int MyProperty1
    {
        get { return GetValue(MyPropertyProperty1); }
        set { SetValue(MyPropertyProperty1, value); }
    }

    /// <summary>
    /// 依赖项
    /// </summary>
    public static readonly DependencyProperty<int> MyPropertyProperty1 =
        new DependencyProperty<int>("MyProperty1", 10);
}
#endregion

#region 依赖属性扩展声明类定义
public static class DependencyExtensions
{
    /// <summary>
    /// 依赖项
    /// </summary>
    public static readonly DependencyProperty<int> MyPropertyProperty2 =
        new DependencyProperty<int>("MyProperty2", 10);

    /// <summary>
    /// 设置MyProperty2
    /// </summary>
    public static TClient SetMyProperty2<TClient>(this TClient client, int value) where TClient : IDependencyObject
    {
        client.SetValue(MyPropertyProperty2, value);
        return client;
    }

    /// <summary>
    /// 获取MyProperty2
    /// </summary>
    public static int GetMyProperty2<TClient>(this TClient client) where TClient : IDependencyObject
    {
        return client.GetValue(MyPropertyProperty2);
    }
}
#endregion

#region 依赖属性应用场景Person类定义
public class Person : DependencyObject
{
    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
}
#endregion

#region 依赖属性应用场景扩展定义
public static class PersonExtensions
{
    /// <summary>
    /// Name依赖项
    /// </summary>
    public static readonly DependencyProperty<string> NameProperty =
        new DependencyProperty<string>("Name", string.Empty);

    /// <summary>
    /// Address依赖项
    /// </summary>
    public static readonly DependencyProperty<string> AddressProperty =
        new DependencyProperty<string>("Address", string.Empty);

    /// <summary>
    /// 设置Name
    /// </summary>
    public static TClient SetName<TClient>(this TClient client, string value) where TClient : IDependencyObject
    {
        client.SetValue(NameProperty, value);
        return client;
    }

    /// <summary>
    /// 获取Name
    /// </summary>
    public static string GetName<TClient>(this TClient client) where TClient : IDependencyObject
    {
        return client.GetValue(NameProperty);
    }

    /// <summary>
    /// 设置Address
    /// </summary>
    public static TClient SetAddress<TClient>(this TClient client, string value) where TClient : IDependencyObject
    {
        client.SetValue(AddressProperty, value);
        return client;
    }

    /// <summary>
    /// 获取Address
    /// </summary>
    public static string GetAddress<TClient>(this TClient client) where TClient : IDependencyObject
    {
        return client.GetValue(AddressProperty);
    }
}
#endregion
