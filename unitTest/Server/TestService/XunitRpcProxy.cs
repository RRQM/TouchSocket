//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace RpcProxy
{
public interface IXUnitTestController:IRemoteServer
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 GET_Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> GET_SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void GET_Test01_Performance(IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task GET_Test01_PerformanceAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
ProxyClass1 GET_Test03_GetProxyClass(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<ProxyClass1> GET_Test03_GetProxyClassAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Collections.Generic.List<Class01> GET_Test14_ListClass01(System.Int32 length,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Collections.Generic.List<Class01>> GET_Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Args GET_Test15_ReturnArgs(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Args> GET_Test15_ReturnArgsAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
MyClass POST_Test29_TestPost(System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyClass> POST_Test29_TestPostAsync(System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test31_WebSocket(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test31_WebSocketAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test01_Performance(IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test01_PerformanceAsync(IInvokeOption invokeOption = default);

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test02_TaskString(System.String msg,IInvokeOption invokeOption = default);
///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test02_TaskStringAsync(System.String msg,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
ProxyClass1 Test03_GetProxyClass(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<ProxyClass1> Test03_GetProxyClassAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test04_In32DefaultValue(System.Int32 a=100,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test04_In32DefaultValueAsync(System.Int32 a=100,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test05_NoneReturnNoneParameter(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test05_NoneReturnNoneParameterAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Boolean Test09_Boolean(System.Boolean b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> Test09_BooleanAsync(System.Boolean b,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test10_StringDefaultNullValue(System.String s=null,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test10_StringDefaultNullValueAsync(System.String s=null,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test11_StringDefaultValue(System.String s="RRQM",IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test11_StringDefaultValueAsync(System.String s="RRQM",IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test13_Task(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test13_TaskAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Collections.Generic.List<Class01> Test14_ListClass01(System.Int32 length,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Collections.Generic.List<Class01>> Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Args Test15_ReturnArgs(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Args> Test15_ReturnArgsAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Class04 Test16_ReturnClass4(System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Class04> Test16_ReturnClass4Async(System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Double Test17_DoubleDefaultValue(System.Double a=3.1415926,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Double> Test17_DoubleDefaultValueAsync(System.Double a=3.1415926,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Class01 Test18_Class1(Class01 class01,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Class01> Test18_Class1Async(Class01 class01,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test20_XmlRpc(System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test20_XmlRpcAsync(System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Collections.Generic.Dictionary<System.Int32,System.String> Test12_Dictionary(System.Int32 length,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Collections.Generic.Dictionary<System.Int32,System.String>> Test12_DictionaryAsync(System.Int32 length,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test22_IncludeCaller(System.Int32 a,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test22_IncludeCallerAsync(System.Int32 a,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test06_OutParameters(out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test07_OutStringParameter(out System.String name,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test08_RefStringParameter(ref System.String name,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test19_CallBacktcpRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test19_CallBacktcpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test23_InvokeType(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test23_InvokeTypeAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test25_TestStruct(StructArgs structArgs,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test25_TestStructAsync(StructArgs structArgs,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test26_TestCancellationToken(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test26_TestCancellationTokenAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test27_TestCallBackFromCallContext(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test27_TestCallBackFromCallContextAsync(IInvokeOption invokeOption = default);

///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test28_TestChannel(System.Int32 channelID,IInvokeOption invokeOption = default);
///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test28_TestChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test30_CallBackHttpRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test30_CallBackHttpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Test31_DefaultBool(System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test31_DefaultBoolAsync(System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default);

}
public class XUnitTestController :IXUnitTestController
{
public XUnitTestController(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 GET_Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=Client.Invoke<System.Int32>("GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> GET_SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return Client.InvokeAsync<System.Int32>("GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void GET_Test01_Performance(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public Task GET_Test01_PerformanceAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public ProxyClass1 GET_Test03_GetProxyClass(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ProxyClass1 returnData=Client.Invoke<ProxyClass1>("GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<ProxyClass1> GET_Test03_GetProxyClassAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<ProxyClass1>("GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Collections.Generic.List<Class01> GET_Test14_ListClass01(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=Client.Invoke<System.Collections.Generic.List<Class01>>("GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Collections.Generic.List<Class01>> GET_Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return Client.InvokeAsync<System.Collections.Generic.List<Class01>>("GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Args GET_Test15_ReturnArgs(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Args returnData=Client.Invoke<Args>("GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<Args> GET_Test15_ReturnArgsAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<Args>("GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public MyClass POST_Test29_TestPost(System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,myClass};
MyClass returnData=Client.Invoke<MyClass>("POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<MyClass> POST_Test29_TestPostAsync(System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,myClass};
return Client.InvokeAsync<MyClass>("POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test31_WebSocket(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("GET:/xunittestcontroller/test31_websocket",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task Test31_WebSocketAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("GET:/xunittestcontroller/test31_websocket",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.sum",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test01_Performance(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public Task Test01_PerformanceAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test01_performance",invokeOption, null);
}

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test02_TaskString(System.String msg,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{msg};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public Task<System.String> Test02_TaskStringAsync(System.String msg,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{msg};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public ProxyClass1 Test03_GetProxyClass(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ProxyClass1 returnData=Client.Invoke<ProxyClass1>("rrqmservice.xunittest.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<ProxyClass1> Test03_GetProxyClassAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<ProxyClass1>("rrqmservice.xunittest.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test04_In32DefaultValue(System.Int32 a=100,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> Test04_In32DefaultValueAsync(System.Int32 a=100,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test05_NoneReturnNoneParameter(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task Test05_NoneReturnNoneParameterAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Boolean Test09_Boolean(System.Boolean b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{b};
System.Boolean returnData=Client.Invoke<System.Boolean>("rrqmservice.xunittest.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Boolean> Test09_BooleanAsync(System.Boolean b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{b};
return Client.InvokeAsync<System.Boolean>("rrqmservice.xunittest.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test10_StringDefaultNullValue(System.String s=null,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test10_StringDefaultNullValueAsync(System.String s=null,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test11_StringDefaultValue(System.String s="RRQM",IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test11_StringDefaultValueAsync(System.String s="RRQM",IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test13_Task(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test13_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task Test13_TaskAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test13_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Collections.Generic.List<Class01> Test14_ListClass01(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=Client.Invoke<System.Collections.Generic.List<Class01>>("rrqmservice.xunittest.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Collections.Generic.List<Class01>> Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return Client.InvokeAsync<System.Collections.Generic.List<Class01>>("rrqmservice.xunittest.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Args Test15_ReturnArgs(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Args returnData=Client.Invoke<Args>("rrqmservice.xunittest.server.xunittestcontroller.test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<Args> Test15_ReturnArgsAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<Args>("rrqmservice.xunittest.server.xunittestcontroller.test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Class04 Test16_ReturnClass4(System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b,c};
Class04 returnData=Client.Invoke<Class04>("rrqmservice.xunittest.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<Class04> Test16_ReturnClass4Async(System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b,c};
return Client.InvokeAsync<Class04>("rrqmservice.xunittest.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Double Test17_DoubleDefaultValue(System.Double a=3.1415926,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Double returnData=Client.Invoke<System.Double>("rrqmservice.xunittest.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Double> Test17_DoubleDefaultValueAsync(System.Double a=3.1415926,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return Client.InvokeAsync<System.Double>("rrqmservice.xunittest.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Class01 Test18_Class1(Class01 class01,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{class01};
Class01 returnData=Client.Invoke<Class01>("rrqmservice.xunittest.server.xunittestcontroller.test18_class1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<Class01> Test18_Class1Async(Class01 class01,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{class01};
return Client.InvokeAsync<Class01>("rrqmservice.xunittest.server.xunittestcontroller.test18_class1",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test20_XmlRpc(System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{param,a,b,args};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test20_XmlRpcAsync(System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{param,a,b,args};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject returnData=Client.Invoke<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("rrqmservice.xunittest.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("rrqmservice.xunittest.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Collections.Generic.Dictionary<System.Int32,System.String> Test12_Dictionary(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.Dictionary<System.Int32,System.String> returnData=Client.Invoke<System.Collections.Generic.Dictionary<System.Int32,System.String>>("rrqmservice.xunittest.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Collections.Generic.Dictionary<System.Int32,System.String>> Test12_DictionaryAsync(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return Client.InvokeAsync<System.Collections.Generic.Dictionary<System.Int32,System.String>>("rrqmservice.xunittest.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test22_IncludeCaller(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> Test22_IncludeCallerAsync(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test06_OutParameters(out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
age=(System.Int32)parameters[1];
occupation=(System.String)parameters[2];
}
else
{
name=default(System.String);
age=default(System.Int32);
occupation=default(System.String);
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test07_OutStringParameter(out System.String name,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
else
{
name=default(System.String);
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test08_RefStringParameter(ref System.String name,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test19_CallBacktcpRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test19_CallBacktcpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test23_InvokeType(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test23_invoketype",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> Test23_InvokeTypeAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test23_invoketype",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test25_TestStruct(StructArgs structArgs,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{structArgs};
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> Test25_TestStructAsync(StructArgs structArgs,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{structArgs};
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test26_TestCancellationToken(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
System.Int32 returnData=Client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> Test26_TestCancellationTokenAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test27_TestCallBackFromCallContext(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task Test27_TestCallBackFromCallContextAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}

///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test28_TestChannel(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{channelID};
Client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}
///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
public Task Test28_TestChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{channelID};
return Client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test30_CallBackHttpRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test30_CallBackHttpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test31_DefaultBool(System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.String returnData=Client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> Test31_DefaultBoolAsync(System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return Client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
}

}
public static class XUnitTestControllerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 GET_Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=client.Invoke<System.Int32>("GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> GET_SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return client.InvokeAsync<System.Int32>("GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void GET_Test01_Performance<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public static Task GET_Test01_PerformanceAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static ProxyClass1 GET_Test03_GetProxyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ProxyClass1 returnData=client.Invoke<ProxyClass1>("GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<ProxyClass1> GET_Test03_GetProxyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<ProxyClass1>("GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.List<Class01> GET_Test14_ListClass01<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=client.Invoke<System.Collections.Generic.List<Class01>>("GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Collections.Generic.List<Class01>> GET_Test14_ListClass01Async<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return client.InvokeAsync<System.Collections.Generic.List<Class01>>("GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Args GET_Test15_ReturnArgs<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Args returnData=client.Invoke<Args>("GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<Args> GET_Test15_ReturnArgsAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<Args>("GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyClass POST_Test29_TestPost<TClient>(this TClient client,System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,myClass};
MyClass returnData=client.Invoke<MyClass>("POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<MyClass> POST_Test29_TestPostAsync<TClient>(this TClient client,System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,myClass};
return client.InvokeAsync<MyClass>("POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test31_WebSocket<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("GET:/xunittestcontroller/test31_websocket",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test31_WebSocketAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.WebApi.IWebApiClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("GET:/xunittestcontroller/test31_websocket",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.sum",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test01_Performance<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public static Task Test01_PerformanceAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test01_performance",invokeOption, null);
}

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test02_TaskString<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{msg};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public static Task<System.String> Test02_TaskStringAsync<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{msg};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static ProxyClass1 Test03_GetProxyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ProxyClass1 returnData=client.Invoke<ProxyClass1>("rrqmservice.xunittest.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<ProxyClass1> Test03_GetProxyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<ProxyClass1>("rrqmservice.xunittest.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test04_In32DefaultValue<TClient>(this TClient client,System.Int32 a=100,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> Test04_In32DefaultValueAsync<TClient>(this TClient client,System.Int32 a=100,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test05_NoneReturnNoneParameter<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test05_NoneReturnNoneParameterAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Boolean Test09_Boolean<TClient>(this TClient client,System.Boolean b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{b};
System.Boolean returnData=client.Invoke<System.Boolean>("rrqmservice.xunittest.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Boolean> Test09_BooleanAsync<TClient>(this TClient client,System.Boolean b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{b};
return client.InvokeAsync<System.Boolean>("rrqmservice.xunittest.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test10_StringDefaultNullValue<TClient>(this TClient client,System.String s=null,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test10_StringDefaultNullValueAsync<TClient>(this TClient client,System.String s=null,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test11_StringDefaultValue<TClient>(this TClient client,System.String s="RRQM",IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test11_StringDefaultValueAsync<TClient>(this TClient client,System.String s="RRQM",IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{s};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test13_Task<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test13_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test13_TaskAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test13_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.List<Class01> Test14_ListClass01<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=client.Invoke<System.Collections.Generic.List<Class01>>("rrqmservice.xunittest.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Collections.Generic.List<Class01>> Test14_ListClass01Async<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return client.InvokeAsync<System.Collections.Generic.List<Class01>>("rrqmservice.xunittest.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Args Test15_ReturnArgs<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
Args returnData=client.Invoke<Args>("rrqmservice.xunittest.server.xunittestcontroller.test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<Args> Test15_ReturnArgsAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<Args>("rrqmservice.xunittest.server.xunittestcontroller.test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Class04 Test16_ReturnClass4<TClient>(this TClient client,System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b,c};
Class04 returnData=client.Invoke<Class04>("rrqmservice.xunittest.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<Class04> Test16_ReturnClass4Async<TClient>(this TClient client,System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b,c};
return client.InvokeAsync<Class04>("rrqmservice.xunittest.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Double Test17_DoubleDefaultValue<TClient>(this TClient client,System.Double a=3.1415926,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Double returnData=client.Invoke<System.Double>("rrqmservice.xunittest.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Double> Test17_DoubleDefaultValueAsync<TClient>(this TClient client,System.Double a=3.1415926,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return client.InvokeAsync<System.Double>("rrqmservice.xunittest.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Class01 Test18_Class1<TClient>(this TClient client,Class01 class01,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{class01};
Class01 returnData=client.Invoke<Class01>("rrqmservice.xunittest.server.xunittestcontroller.test18_class1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<Class01> Test18_Class1Async<TClient>(this TClient client,Class01 class01,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{class01};
return client.InvokeAsync<Class01>("rrqmservice.xunittest.server.xunittestcontroller.test18_class1",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test20_XmlRpc<TClient>(this TClient client,System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{param,a,b,args};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test20_XmlRpcAsync<TClient>(this TClient client,System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{param,a,b,args};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject returnData=client.Invoke<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("rrqmservice.xunittest.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.XmlRpc.IXmlRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("rrqmservice.xunittest.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.Dictionary<System.Int32,System.String> Test12_Dictionary<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
System.Collections.Generic.Dictionary<System.Int32,System.String> returnData=client.Invoke<System.Collections.Generic.Dictionary<System.Int32,System.String>>("rrqmservice.xunittest.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Collections.Generic.Dictionary<System.Int32,System.String>> Test12_DictionaryAsync<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{length};
return client.InvokeAsync<System.Collections.Generic.Dictionary<System.Int32,System.String>>("rrqmservice.xunittest.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test22_IncludeCaller<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> Test22_IncludeCallerAsync<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test06_OutParameters<TClient>(this TClient client,out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
age=(System.Int32)parameters[1];
occupation=(System.String)parameters[2];
}
else
{
name=default(System.String);
age=default(System.Int32);
occupation=default(System.String);
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test07_OutStringParameter<TClient>(this TClient client,out System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
else
{
name=default(System.String);
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test08_RefStringParameter<TClient>(this TClient client,ref System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test19_CallBacktcpRpcService<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test19_CallBacktcpRpcServiceAsync<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test23_InvokeType<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test23_invoketype",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> Test23_InvokeTypeAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test23_invoketype",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test25_TestStruct<TClient>(this TClient client,StructArgs structArgs,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{structArgs};
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> Test25_TestStructAsync<TClient>(this TClient client,StructArgs structArgs,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{structArgs};
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test26_TestCancellationToken<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
System.Int32 returnData=client.Invoke<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> Test26_TestCancellationTokenAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<System.Int32>("rrqmservice.xunittest.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test27_TestCallBackFromCallContext<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test27_TestCallBackFromCallContextAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}

///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test28_TestChannel<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{channelID};
client.Invoke("rrqmservice.xunittest.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}
///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
public static Task Test28_TestChannelAsync<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{channelID};
return client.InvokeAsync("rrqmservice.xunittest.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test30_CallBackHttpRpcService<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test30_CallBackHttpRpcServiceAsync<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{id,age};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test31_DefaultBool<TClient>(this TClient client,System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.String returnData=client.Invoke<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> Test31_DefaultBoolAsync<TClient>(this TClient client,System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return client.InvokeAsync<System.String>("rrqmservice.xunittest.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
}

}
public interface IOtherAssemblyServer:IRemoteServer
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
ClassOne GetClassOne(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<ClassOne> GetClassOneAsync(IInvokeOption invokeOption = default);

}
public class OtherAssemblyServer :IOtherAssemblyServer
{
public OtherAssemblyServer(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=Client.Invoke<System.Int32>("rpcargsclasslib.iotherassemblyserver.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return Client.InvokeAsync<System.Int32>("rpcargsclasslib.iotherassemblyserver.sum",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public ClassOne GetClassOne(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ClassOne returnData=Client.Invoke<ClassOne>("rpcargsclasslib.iotherassemblyserver.getclassone",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<ClassOne> GetClassOneAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return Client.InvokeAsync<ClassOne>("rpcargsclasslib.iotherassemblyserver.getclassone",invokeOption, null);
}

}
public static class OtherAssemblyServerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=client.Invoke<System.Int32>("rpcargsclasslib.iotherassemblyserver.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a,b};
return client.InvokeAsync<System.Int32>("rpcargsclasslib.iotherassemblyserver.sum",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static ClassOne GetClassOne<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
ClassOne returnData=client.Invoke<ClassOne>("rpcargsclasslib.iotherassemblyserver.getclassone",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<ClassOne> GetClassOneAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
return client.InvokeAsync<ClassOne>("rpcargsclasslib.iotherassemblyserver.getclassone",invokeOption, null);
}

}

public class ProxyClass3
{
public System.Int32 P1{get;set;}
}


public class ProxyClass2
{
public System.Int32 P1{get;set;}
public ProxyClass3 P2{get;set;}
}


public class ProxyClass1
{
public System.Int32 P1{get;set;}
public ProxyClass2 P2{get;set;}
}


public class Class01
{
public System.Int32 Age{get;set;}
public System.String Name{get;set;}
}


public class Args
{
public System.Int32 P1{get;set;}
public System.Double P2{get;set;}
public System.String P3{get;set;}
}


public class MyClass
{
public System.Int32 P1{get;set;}
}


public class Class04
{
public System.Int32 P1{get;set;}
public System.String P2{get;set;}
public System.Int32 P3{get;set;}
}


public class ClassOne
{
public System.Int32 P1{get;set;}
public System.String P2{get;set;}
}


public struct StructArgs
{
public System.Int32 P1{get;set;}
}

}
