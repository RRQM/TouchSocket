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

using TouchSocket.Core;
using TouchSocket.Rpc;

namespace GenerateProxyFromServerConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        #region 代理生成基本示例

        var rpcStore = new RpcStore(new Container());

        // 注册RPC服务
        rpcStore.RegisterServer<MyRpcClass>();

        // 生成代理代码并写入文件
        var proxyCodes = rpcStore.GetProxyCodes("RpcProxy", new Type[] { typeof(MyRpcAttribute) });
        File.WriteAllText("RpcProxy.cs", proxyCodes);
        Console.WriteLine("代理代码已生成到 RpcProxy.cs 文件");

        #endregion

        #region 代理生成直接添加代理类型

        // 直接添加代理类型
        CodeGenerator.AddProxyType<ProxyClass1>();
        CodeGenerator.AddProxyType<ProxyClass2>(deepSearch: true);

        #endregion

        #region 代理生成按程序集添加

        // 按程序集添加代理类型
        CodeGenerator.AddProxyAssembly(typeof(Program).Assembly);

        #endregion

        #region 代理生成类型排除

        // 排除特定类型
        CodeGenerator.AddIgnoreProxyType(typeof(Program));

        #endregion

        #region 代理生成按程序集排除

        // 按程序集排除
        CodeGenerator.AddIgnoreProxyAssembly(typeof(Program).Assembly);

        #endregion

        #region 代理生成使用CodeGenerator静态类

        // 使用CodeGenerator直接生成代理代码
        string codes = CodeGenerator.GetProxyCodes("Namespace", new Type[] { typeof(MyRpcClass) }, new Type[] { typeof(MyRpcAttribute) });
        Console.WriteLine("使用CodeGenerator生成的代理代码:");
        Console.WriteLine(codes);

        #endregion

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}

internal partial class MyRpcClass : SingletonRpcServer
{
    [MyRpc]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [MyRpc]
    public string GetMessage(string name)
    {
        return $"Hello, {name}!";
    }
}

#region 代理生成自定义RpcAttribute示例

internal class MyRpcAttribute : RpcAttribute
{
    public MyRpcAttribute()
    {
        this.GeneratorFlag = CodeGeneratorFlag.ExtensionAsync | CodeGeneratorFlag.InstanceAsync;
    }

    public override Type[] GetGenericConstraintTypes()
    {
        return new Type[] { typeof(IRpcClient) };
    }

    public override string GetDescription(RpcMethod methodInstance)
    {
        return base.GetDescription(methodInstance);
    }
}

#endregion

#region 代理生成通过特性标记添加

[RpcProxy("MyProxyArgs")]
public class ProxyClass1
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

#endregion

public class ProxyClass2
{
    public string? Description { get; set; }
    public ProxyClass1? NestedClass { get; set; }
}