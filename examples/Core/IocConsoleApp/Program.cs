using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;

namespace IocConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NormalContainer.Run();// 常规Ioc容器，使用IL和反射实现，支持运行时注册和获取
            SourceGeneratorContainer.Run();// 源生成Ioc容器，单例实例支持运行时注册和获取，其他模式只能在编码阶段自动生成。
            Console.ReadKey();
        }

       
    }
}