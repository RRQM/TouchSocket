/*
此代码由Rpc工具直接生成，非必要请不要修改此处代码
*/
#pragma warning disable
using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace RpcProxy
{
public interface IXUnitTestController:TouchSocket.Rpc.IRemoteServer
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
void Test06_OutParameters(out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test06_OutParametersAsync(out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default);

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
Task Test07_OutStringParameterAsync(out System.String name,IInvokeOption invokeOption = default);

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
Task Test08_RefStringParameterAsync(ref System.String name,IInvokeOption invokeOption = default);

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
Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync(IInvokeOption invokeOption = default);

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

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
(System.Int32 a,System.String b) Test33_TupleElementNames((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<(System.Int32 a,System.String b)> Test33_TupleElementNamesAsync((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
(System.Int32 a,System.String b) Test34_RefTupleElementNames(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<(System.Int32 a,System.String b)> Test34_RefTupleElementNamesAsync(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test35_ListTupleElementNames(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test35_ListTupleElementNamesAsync(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test36_ListRefTupleElementNames(ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test36_ListRefTupleElementNamesAsync(ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test37_ListTupleElementNames2(System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test37_ListTupleElementNames2Async(System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test39_Task(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test39_TaskAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
TouchSocket.Core.Metadata Test40_CallContextMetadata(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<TouchSocket.Core.Metadata> Test40_CallContextMetadataAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Test41_RateLimiting(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> Test41_RateLimitingAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Collections.Generic.List<System.Int32> Test42_TaskList(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Collections.Generic.List<System.Int32>> Test42_TaskListAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
(System.Int32 a,System.String b) Test43_TaskValueTuple(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<(System.Int32 a,System.String b)> Test43_TaskValueTupleAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test45(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test45Async(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test46(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test46Async(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Test47(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task Test47Async(System.String id,IInvokeOption invokeOption = default);

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
System.String Test38_CallBackJsonRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> Test38_CallBackJsonRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default);

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
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String POST_Test02_TaskString(System.String msg,IInvokeOption invokeOption = default);
///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> POST_Test02_TaskStringAsync(System.String msg,IInvokeOption invokeOption = default);

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
(System.Int32 a,System.String b) POST_Test33_TupleElementNames((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<(System.Int32 a,System.String b)> POST_Test33_TupleElementNamesAsync((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
(System.Int32 a,System.String b) POST_Test34_RefTupleElementNames(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<(System.Int32 a,System.String b)> POST_Test34_RefTupleElementNamesAsync(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void POST_Test39_Task(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task POST_Test39_TaskAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void GET_Test45(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task GET_Test45Async(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void GET_Test46(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task GET_Test46Async(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void GET_Test47(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task GET_Test47Async(System.String id,IInvokeOption invokeOption = default);

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
public System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.sum",invokeOption, parameters);
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
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test01_performance",invokeOption, null);
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
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test01_performance",invokeOption, null);
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
object[] parameters = new object[]{msg};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public async Task<System.String> Test02_TaskStringAsync(System.String msg,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{msg};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
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
object[] parameters = new object[]{a};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test04_In32DefaultValueAsync(System.Int32 a=100,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
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
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
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
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
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
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
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
public Task Test06_OutParametersAsync(out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
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
return  EasyTask.CompletedTask;
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
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
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
public Task Test07_OutStringParameterAsync(out System.String name,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
else
{
name=default(System.String);
}
return  EasyTask.CompletedTask;
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
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
}
///<summary>
///无注释信息
///</summary>
public Task Test08_RefStringParameterAsync(ref System.String name,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
return  EasyTask.CompletedTask;
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
object[] parameters = new object[]{b};
System.Boolean returnData=(System.Boolean)Client.Invoke(typeof(System.Boolean),"xunittestconsoleapp.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Boolean> Test09_BooleanAsync(System.Boolean b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{b};
return (System.Boolean) await Client.InvokeAsync(typeof(System.Boolean),"xunittestconsoleapp.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
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
object[] parameters = new object[]{s};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test10_StringDefaultNullValueAsync(System.String s=null,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{s};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
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
object[] parameters = new object[]{s};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test11_StringDefaultValueAsync(System.String s="RRQM",IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{s};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
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
object[] parameters = new object[]{length};
System.Collections.Generic.Dictionary<System.Int32,System.String> returnData=(System.Collections.Generic.Dictionary<System.Int32,System.String>)Client.Invoke(typeof(System.Collections.Generic.Dictionary<System.Int32,System.String>),"xunittestconsoleapp.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Collections.Generic.Dictionary<System.Int32,System.String>> Test12_DictionaryAsync(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{length};
return (System.Collections.Generic.Dictionary<System.Int32,System.String>) await Client.InvokeAsync(typeof(System.Collections.Generic.Dictionary<System.Int32,System.String>),"xunittestconsoleapp.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
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
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test13_task",invokeOption, null);
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
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test13_task",invokeOption, null);
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
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=(System.Collections.Generic.List<Class01>)Client.Invoke(typeof(System.Collections.Generic.List<Class01>),"xunittestconsoleapp.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Collections.Generic.List<Class01>> Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{length};
return (System.Collections.Generic.List<Class01>) await Client.InvokeAsync(typeof(System.Collections.Generic.List<Class01>),"xunittestconsoleapp.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
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
Args returnData=(Args)Client.Invoke(typeof(Args),"xunittestconsoleapp.server.xunittestcontroller.test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Args> Test15_ReturnArgsAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (Args) await Client.InvokeAsync(typeof(Args),"xunittestconsoleapp.server.xunittestcontroller.test15_returnargs",invokeOption, null);
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
object[] parameters = new object[]{a,b,c};
Class04 returnData=(Class04)Client.Invoke(typeof(Class04),"xunittestconsoleapp.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Class04> Test16_ReturnClass4Async(System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b,c};
return (Class04) await Client.InvokeAsync(typeof(Class04),"xunittestconsoleapp.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
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
object[] parameters = new object[]{a};
System.Double returnData=(System.Double)Client.Invoke(typeof(System.Double),"xunittestconsoleapp.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Double> Test17_DoubleDefaultValueAsync(System.Double a=3.1415926,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a};
return (System.Double) await Client.InvokeAsync(typeof(System.Double),"xunittestconsoleapp.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
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
object[] parameters = new object[]{class01};
Class01 returnData=(Class01)Client.Invoke(typeof(Class01),"xunittestconsoleapp.server.xunittestcontroller.test18_class1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Class01> Test18_Class1Async(Class01 class01,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{class01};
return (Class01) await Client.InvokeAsync(typeof(Class01),"xunittestconsoleapp.server.xunittestcontroller.test18_class1",invokeOption, parameters);
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
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test19_CallBacktcpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,age};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
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
object[] parameters = new object[]{param,a,b,args};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test20_XmlRpcAsync(System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{param,a,b,args};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
Newtonsoft.Json.Linq.JObject returnData=(Newtonsoft.Json.Linq.JObject)Client.Invoke(typeof(Newtonsoft.Json.Linq.JObject),"xunittestconsoleapp.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (Newtonsoft.Json.Linq.JObject) await Client.InvokeAsync(typeof(Newtonsoft.Json.Linq.JObject),"xunittestconsoleapp.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
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
object[] parameters = new object[]{a};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test22_IncludeCallerAsync(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
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
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test23_invoketype",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test23_InvokeTypeAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test23_invoketype",invokeOption, null);
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
object[] parameters = new object[]{structArgs};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test25_TestStructAsync(StructArgs structArgs,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{structArgs};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
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
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test26_TestCancellationTokenAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
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
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
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
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
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
object[] parameters = new object[]{channelID};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
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
object[] parameters = new object[]{channelID};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
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
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test30_CallBackHttpRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,age};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
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
object[] parameters = new object[]{a,b};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test31_DefaultBoolAsync(System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public (System.Int32 a,System.String b) Test33_TupleElementNames((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test33_tupleelementnames",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<(System.Int32 a,System.String b)> Test33_TupleElementNamesAsync((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
return ((System.Int32 a,System.String b)) await Client.InvokeAsync(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test33_tupleelementnames",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public (System.Int32 a,System.String b) Test34_RefTupleElementNames(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test34_reftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<(System.Int32 a,System.String b)> Test34_RefTupleElementNamesAsync(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test34_reftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return Task.FromResult<(System.Int32 a,System.String b)>(returnData);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test35_ListTupleElementNames(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test35_listtupleelementnames",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task Test35_ListTupleElementNamesAsync(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test35_listtupleelementnames",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test36_ListRefTupleElementNames(ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test36_listreftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)parameters[0];
}
}
///<summary>
///无注释信息
///</summary>
public Task Test36_ListRefTupleElementNamesAsync(ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test36_listreftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)parameters[0];
}
return  EasyTask.CompletedTask;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test37_ListTupleElementNames2(System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test37_listtupleelementnames2",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task Test37_ListTupleElementNames2Async(System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test37_listtupleelementnames2",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test39_Task(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test39_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task Test39_TaskAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test39_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public TouchSocket.Core.Metadata Test40_CallContextMetadata(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
TouchSocket.Core.Metadata returnData=(TouchSocket.Core.Metadata)Client.Invoke(typeof(TouchSocket.Core.Metadata),"xunittestconsoleapp.server.xunittestcontroller.test40_callcontextmetadata",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<TouchSocket.Core.Metadata> Test40_CallContextMetadataAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (TouchSocket.Core.Metadata) await Client.InvokeAsync(typeof(TouchSocket.Core.Metadata),"xunittestconsoleapp.server.xunittestcontroller.test40_callcontextmetadata",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Test41_RateLimiting(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test41_ratelimiting",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> Test41_RateLimitingAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test41_ratelimiting",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Collections.Generic.List<System.Int32> Test42_TaskList(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
System.Collections.Generic.List<System.Int32> returnData=(System.Collections.Generic.List<System.Int32>)Client.Invoke(typeof(System.Collections.Generic.List<System.Int32>),"xunittestconsoleapp.server.xunittestcontroller.test42_tasklist",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Collections.Generic.List<System.Int32>> Test42_TaskListAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.Collections.Generic.List<System.Int32>) await Client.InvokeAsync(typeof(System.Collections.Generic.List<System.Int32>),"xunittestconsoleapp.server.xunittestcontroller.test42_tasklist",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public (System.Int32 a,System.String b) Test43_TaskValueTuple(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test43_taskvaluetuple",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<(System.Int32 a,System.String b)> Test43_TaskValueTupleAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return ((System.Int32 a,System.String b)) await Client.InvokeAsync(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test43_taskvaluetuple",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test45(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test45",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task Test45Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test45",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test46(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test46",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task Test46Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test46",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Test47(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test47",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task Test47Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test47",invokeOption, parameters);
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
ProxyClass1 returnData=(ProxyClass1)Client.Invoke(typeof(ProxyClass1),"xunittestconsoleapp.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<ProxyClass1> Test03_GetProxyClassAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (ProxyClass1) await Client.InvokeAsync(typeof(ProxyClass1),"xunittestconsoleapp.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Test38_CallBackJsonRpcService(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test38_callbackjsonrpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> Test38_CallBackJsonRpcServiceAsync(System.String id,System.Int32 age,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,age};
return (System.String) await Client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test38_callbackjsonrpcservice",invokeOption, parameters);
}

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
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> GET_SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
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
return Client.InvokeAsync("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String POST_Test02_TaskString(System.String msg,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{msg};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"POST:/xunittestcontroller/test02_taskstring?msg={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public async Task<System.String> POST_Test02_TaskStringAsync(System.String msg,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{msg};
return (System.String) await Client.InvokeAsync(typeof(System.String),"POST:/xunittestcontroller/test02_taskstring?msg={0}",invokeOption, parameters);
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
ProxyClass1 returnData=(ProxyClass1)Client.Invoke(typeof(ProxyClass1),"GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<ProxyClass1> GET_Test03_GetProxyClassAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (ProxyClass1) await Client.InvokeAsync(typeof(ProxyClass1),"GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
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
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=(System.Collections.Generic.List<Class01>)Client.Invoke(typeof(System.Collections.Generic.List<Class01>),"GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Collections.Generic.List<Class01>> GET_Test14_ListClass01Async(System.Int32 length,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{length};
return (System.Collections.Generic.List<Class01>) await Client.InvokeAsync(typeof(System.Collections.Generic.List<Class01>),"GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
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
Args returnData=(Args)Client.Invoke(typeof(Args),"GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Args> GET_Test15_ReturnArgsAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (Args) await Client.InvokeAsync(typeof(Args),"GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
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
object[] parameters = new object[]{a,myClass};
MyClass returnData=(MyClass)Client.Invoke(typeof(MyClass),"POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<MyClass> POST_Test29_TestPostAsync(System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,myClass};
return (MyClass) await Client.InvokeAsync(typeof(MyClass),"POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public (System.Int32 a,System.String b) POST_Test33_TupleElementNames((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test33_tupleelementnames?",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<(System.Int32 a,System.String b)> POST_Test33_TupleElementNamesAsync((System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
return ((System.Int32 a,System.String b)) await Client.InvokeAsync(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test33_tupleelementnames?",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public (System.Int32 a,System.String b) POST_Test34_RefTupleElementNames(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test34_reftupleelementnames?",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<(System.Int32 a,System.String b)> POST_Test34_RefTupleElementNamesAsync(ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))Client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test34_reftupleelementnames?",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return Task.FromResult<(System.Int32 a,System.String b)>(returnData);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void POST_Test39_Task(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
Client.Invoke("POST:/xunittestcontroller/test39_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public Task POST_Test39_TaskAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return Client.InvokeAsync("POST:/xunittestcontroller/test39_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void GET_Test45(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("GET:/xunittestcontroller/test45?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task GET_Test45Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("GET:/xunittestcontroller/test45?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void GET_Test46(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("GET:/xunittestcontroller/test46?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task GET_Test46Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("GET:/xunittestcontroller/test46?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void GET_Test47(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
Client.Invoke("GET:/xunittestcontroller/test47?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public Task GET_Test47Async(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return Client.InvokeAsync("GET:/xunittestcontroller/test47?id={0}",invokeOption, parameters);
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
public static System.Int32 Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.sum",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.sum",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test01_Performance<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public static Task Test01_PerformanceAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test01_performance",invokeOption, null);
}

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test02_TaskString<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{msg};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public static async Task<System.String> Test02_TaskStringAsync<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{msg};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test02_taskstring",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test04_In32DefaultValue<TClient>(this TClient client,System.Int32 a=100,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test04_In32DefaultValueAsync<TClient>(this TClient client,System.Int32 a=100,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test04_in32defaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test05_NoneReturnNoneParameter<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test05_NoneReturnNoneParameterAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test05_nonereturnnoneparameter",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test06_OutParameters<TClient>(this TClient client,out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
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
public static Task Test06_OutParametersAsync<TClient>(this TClient client,out System.String name,out System.Int32 age,out System.String occupation,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{default(System.String),default(System.Int32),default(System.String)};
Type[] types = new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test06_outparameters",invokeOption,ref parameters,types);
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
return  EasyTask.CompletedTask;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test07_OutStringParameter<TClient>(this TClient client,out System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
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
public static Task Test07_OutStringParameterAsync<TClient>(this TClient client,out System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{default(System.String)};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test07_outstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
else
{
name=default(System.String);
}
return  EasyTask.CompletedTask;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test08_RefStringParameter<TClient>(this TClient client,ref System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
}
///<summary>
///无注释信息
///</summary>
public static Task Test08_RefStringParameterAsync<TClient>(this TClient client,ref System.String name,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{name};
Type[] types = new Type[]{typeof(System.String)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test08_refstringparameter",invokeOption,ref parameters,types);
if(parameters!=null)
{
name=(System.String)parameters[0];
}
return  EasyTask.CompletedTask;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Boolean Test09_Boolean<TClient>(this TClient client,System.Boolean b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{b};
System.Boolean returnData=(System.Boolean)client.Invoke(typeof(System.Boolean),"xunittestconsoleapp.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Boolean> Test09_BooleanAsync<TClient>(this TClient client,System.Boolean b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{b};
return (System.Boolean) await client.InvokeAsync(typeof(System.Boolean),"xunittestconsoleapp.server.xunittestcontroller.test09_boolean",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test10_StringDefaultNullValue<TClient>(this TClient client,System.String s=null,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{s};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test10_StringDefaultNullValueAsync<TClient>(this TClient client,System.String s=null,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{s};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test10_stringdefaultnullvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test11_StringDefaultValue<TClient>(this TClient client,System.String s="RRQM",IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{s};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test11_StringDefaultValueAsync<TClient>(this TClient client,System.String s="RRQM",IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{s};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test11_stringdefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.Dictionary<System.Int32,System.String> Test12_Dictionary<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{length};
System.Collections.Generic.Dictionary<System.Int32,System.String> returnData=(System.Collections.Generic.Dictionary<System.Int32,System.String>)client.Invoke(typeof(System.Collections.Generic.Dictionary<System.Int32,System.String>),"xunittestconsoleapp.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Collections.Generic.Dictionary<System.Int32,System.String>> Test12_DictionaryAsync<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{length};
return (System.Collections.Generic.Dictionary<System.Int32,System.String>) await client.InvokeAsync(typeof(System.Collections.Generic.Dictionary<System.Int32,System.String>),"xunittestconsoleapp.server.xunittestcontroller.test12_dictionary",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test13_Task<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test13_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test13_TaskAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test13_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.List<Class01> Test14_ListClass01<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=(System.Collections.Generic.List<Class01>)client.Invoke(typeof(System.Collections.Generic.List<Class01>),"xunittestconsoleapp.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Collections.Generic.List<Class01>> Test14_ListClass01Async<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{length};
return (System.Collections.Generic.List<Class01>) await client.InvokeAsync(typeof(System.Collections.Generic.List<Class01>),"xunittestconsoleapp.server.xunittestcontroller.test14_listclass01",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Args Test15_ReturnArgs<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
Args returnData=(Args)client.Invoke(typeof(Args),"xunittestconsoleapp.server.xunittestcontroller.test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Args> Test15_ReturnArgsAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (Args) await client.InvokeAsync(typeof(Args),"xunittestconsoleapp.server.xunittestcontroller.test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Class04 Test16_ReturnClass4<TClient>(this TClient client,System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b,c};
Class04 returnData=(Class04)client.Invoke(typeof(Class04),"xunittestconsoleapp.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Class04> Test16_ReturnClass4Async<TClient>(this TClient client,System.Int32 a,System.String b,System.Int32 c=10,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b,c};
return (Class04) await client.InvokeAsync(typeof(Class04),"xunittestconsoleapp.server.xunittestcontroller.test16_returnclass4",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Double Test17_DoubleDefaultValue<TClient>(this TClient client,System.Double a=3.1415926,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
System.Double returnData=(System.Double)client.Invoke(typeof(System.Double),"xunittestconsoleapp.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Double> Test17_DoubleDefaultValueAsync<TClient>(this TClient client,System.Double a=3.1415926,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
return (System.Double) await client.InvokeAsync(typeof(System.Double),"xunittestconsoleapp.server.xunittestcontroller.test17_doubledefaultvalue",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Class01 Test18_Class1<TClient>(this TClient client,Class01 class01,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{class01};
Class01 returnData=(Class01)client.Invoke(typeof(Class01),"xunittestconsoleapp.server.xunittestcontroller.test18_class1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Class01> Test18_Class1Async<TClient>(this TClient client,Class01 class01,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{class01};
return (Class01) await client.InvokeAsync(typeof(Class01),"xunittestconsoleapp.server.xunittestcontroller.test18_class1",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test19_CallBacktcpRpcService<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test19_CallBacktcpRpcServiceAsync<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id,age};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test19_callbacktcprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test20_XmlRpc<TClient>(this TClient client,System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{param,a,b,args};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test20_XmlRpcAsync<TClient>(this TClient client,System.String param,System.Int32 a,System.Double b,Args[] args,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{param,a,b,args};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test20_xmlrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Newtonsoft.Json.Linq.JObject Test21_JsonRpcReturnJObject<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
Newtonsoft.Json.Linq.JObject returnData=(Newtonsoft.Json.Linq.JObject)client.Invoke(typeof(Newtonsoft.Json.Linq.JObject),"xunittestconsoleapp.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Newtonsoft.Json.Linq.JObject> Test21_JsonRpcReturnJObjectAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (Newtonsoft.Json.Linq.JObject) await client.InvokeAsync(typeof(Newtonsoft.Json.Linq.JObject),"xunittestconsoleapp.server.xunittestcontroller.test21_jsonrpcreturnjobject",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test22_IncludeCaller<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test22_IncludeCallerAsync<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test22_includecaller",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test23_InvokeType<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test23_invoketype",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test23_InvokeTypeAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test23_invoketype",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test25_TestStruct<TClient>(this TClient client,StructArgs structArgs,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{structArgs};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test25_TestStructAsync<TClient>(this TClient client,StructArgs structArgs,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{structArgs};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test25_teststruct",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test26_TestCancellationToken<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test26_TestCancellationTokenAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test26_testcancellationtoken",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test27_TestCallBackFromCallContext<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test27_TestCallBackFromCallContextAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test27_testcallbackfromcallcontext",invokeOption, null);
}

///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test28_TestChannel<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}
///<summary>
///测试从RPC创建通道，从而实现流数据的传输
///</summary>
public static Task Test28_TestChannelAsync<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test28_testchannel",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test30_CallBackHttpRpcService<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test30_CallBackHttpRpcServiceAsync<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id,age};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test30_callbackhttprpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test31_DefaultBool<TClient>(this TClient client,System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test31_DefaultBoolAsync<TClient>(this TClient client,System.Boolean a=true,System.Boolean b=false,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test31_defaultbool",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static (System.Int32 a,System.String b) Test33_TupleElementNames<TClient>(this TClient client,(System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test33_tupleelementnames",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<(System.Int32 a,System.String b)> Test33_TupleElementNamesAsync<TClient>(this TClient client,(System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
return ((System.Int32 a,System.String b)) await client.InvokeAsync(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test33_tupleelementnames",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static (System.Int32 a,System.String b) Test34_RefTupleElementNames<TClient>(this TClient client,ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test34_reftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<(System.Int32 a,System.String b)> Test34_RefTupleElementNamesAsync<TClient>(this TClient client,ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test34_reftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return Task.FromResult<(System.Int32 a,System.String b)>(returnData);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test35_ListTupleElementNames<TClient>(this TClient client,System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test35_listtupleelementnames",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task Test35_ListTupleElementNamesAsync<TClient>(this TClient client,System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test35_listtupleelementnames",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test36_ListRefTupleElementNames<TClient>(this TClient client,ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test36_listreftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)parameters[0];
}
}
///<summary>
///无注释信息
///</summary>
public static Task Test36_ListRefTupleElementNamesAsync<TClient>(this TClient client,ref System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test36_listreftupleelementnames",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=(System.Collections.Generic.List<(System.Int32 a1,(System.Int32 a2,System.String a3) a4)>)parameters[0];
}
return  EasyTask.CompletedTask;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test37_ListTupleElementNames2<TClient>(this TClient client,System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test37_listtupleelementnames2",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task Test37_ListTupleElementNames2Async<TClient>(this TClient client,System.Collections.Generic.List<(System.Int32 b1,(System.Int32 b2,System.String b3) b4)> tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{tuple};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test37_listtupleelementnames2",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test39_Task<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test39_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task Test39_TaskAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test39_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static TouchSocket.Core.Metadata Test40_CallContextMetadata<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
TouchSocket.Core.Metadata returnData=(TouchSocket.Core.Metadata)client.Invoke(typeof(TouchSocket.Core.Metadata),"xunittestconsoleapp.server.xunittestcontroller.test40_callcontextmetadata",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<TouchSocket.Core.Metadata> Test40_CallContextMetadataAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (TouchSocket.Core.Metadata) await client.InvokeAsync(typeof(TouchSocket.Core.Metadata),"xunittestconsoleapp.server.xunittestcontroller.test40_callcontextmetadata",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Test41_RateLimiting<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test41_ratelimiting",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> Test41_RateLimitingAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"xunittestconsoleapp.server.xunittestcontroller.test41_ratelimiting",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.List<System.Int32> Test42_TaskList<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
System.Collections.Generic.List<System.Int32> returnData=(System.Collections.Generic.List<System.Int32>)client.Invoke(typeof(System.Collections.Generic.List<System.Int32>),"xunittestconsoleapp.server.xunittestcontroller.test42_tasklist",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Collections.Generic.List<System.Int32>> Test42_TaskListAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (System.Collections.Generic.List<System.Int32>) await client.InvokeAsync(typeof(System.Collections.Generic.List<System.Int32>),"xunittestconsoleapp.server.xunittestcontroller.test42_tasklist",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static (System.Int32 a,System.String b) Test43_TaskValueTuple<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test43_taskvaluetuple",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<(System.Int32 a,System.String b)> Test43_TaskValueTupleAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return ((System.Int32 a,System.String b)) await client.InvokeAsync(typeof((System.Int32 a,System.String b)),"xunittestconsoleapp.server.xunittestcontroller.test43_taskvaluetuple",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test45<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test45",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task Test45Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test45",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test46<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test46",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task Test46Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test46",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Test47<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
client.Invoke("xunittestconsoleapp.server.xunittestcontroller.test47",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task Test47Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{id};
return client.InvokeAsync("xunittestconsoleapp.server.xunittestcontroller.test47",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static ProxyClass1 Test03_GetProxyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
ProxyClass1 returnData=(ProxyClass1)client.Invoke(typeof(ProxyClass1),"xunittestconsoleapp.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<ProxyClass1> Test03_GetProxyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
return (ProxyClass1) await client.InvokeAsync(typeof(ProxyClass1),"xunittestconsoleapp.server.xunittestcontroller.test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Test38_CallBackJsonRpcService<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id,age};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test38_callbackjsonrpcservice",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> Test38_CallBackJsonRpcServiceAsync<TClient>(this TClient client,System.String id,System.Int32 age,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id,age};
return (System.String) await client.InvokeAsync(typeof(System.String),"xunittestconsoleapp.server.xunittestcontroller.test38_callbackjsonrpcservice",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 GET_Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> GET_SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,b};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"GET:/xunittestcontroller/sum?a={0}&b={1}",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void GET_Test01_Performance<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
client.Invoke("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}
///<summary>
///性能测试
///</summary>
public static Task GET_Test01_PerformanceAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return client.InvokeAsync("GET:/xunittestcontroller/test01_performance",invokeOption, null);
}

///<summary>
///测试异步字符串
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String POST_Test02_TaskString<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{msg};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"POST:/xunittestcontroller/test02_taskstring?msg={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///测试异步字符串
///</summary>
public static async Task<System.String> POST_Test02_TaskStringAsync<TClient>(this TClient client,System.String msg,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{msg};
return (System.String) await client.InvokeAsync(typeof(System.String),"POST:/xunittestcontroller/test02_taskstring?msg={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static ProxyClass1 GET_Test03_GetProxyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
ProxyClass1 returnData=(ProxyClass1)client.Invoke(typeof(ProxyClass1),"GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<ProxyClass1> GET_Test03_GetProxyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return (ProxyClass1) await client.InvokeAsync(typeof(ProxyClass1),"GET:/xunittestcontroller/test03_getproxyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Collections.Generic.List<Class01> GET_Test14_ListClass01<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{length};
System.Collections.Generic.List<Class01> returnData=(System.Collections.Generic.List<Class01>)client.Invoke(typeof(System.Collections.Generic.List<Class01>),"GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Collections.Generic.List<Class01>> GET_Test14_ListClass01Async<TClient>(this TClient client,System.Int32 length,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{length};
return (System.Collections.Generic.List<Class01>) await client.InvokeAsync(typeof(System.Collections.Generic.List<Class01>),"GET:/xunittestcontroller/test14_listclass01?length={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Args GET_Test15_ReturnArgs<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
Args returnData=(Args)client.Invoke(typeof(Args),"GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Args> GET_Test15_ReturnArgsAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return (Args) await client.InvokeAsync(typeof(Args),"GET:/xunittestcontroller/test15_returnargs",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyClass POST_Test29_TestPost<TClient>(this TClient client,System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,myClass};
MyClass returnData=(MyClass)client.Invoke(typeof(MyClass),"POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<MyClass> POST_Test29_TestPostAsync<TClient>(this TClient client,System.Int32 a,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,myClass};
return (MyClass) await client.InvokeAsync(typeof(MyClass),"POST:/xunittestcontroller/test29_testpost?a={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static (System.Int32 a,System.String b) POST_Test33_TupleElementNames<TClient>(this TClient client,(System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{tuple};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test33_tupleelementnames?",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<(System.Int32 a,System.String b)> POST_Test33_TupleElementNamesAsync<TClient>(this TClient client,(System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{tuple};
return ((System.Int32 a,System.String b)) await client.InvokeAsync(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test33_tupleelementnames?",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static (System.Int32 a,System.String b) POST_Test34_RefTupleElementNames<TClient>(this TClient client,ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test34_reftupleelementnames?",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<(System.Int32 a,System.String b)> POST_Test34_RefTupleElementNamesAsync<TClient>(this TClient client,ref (System.Int32 a,System.String b) tuple,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{tuple};
Type[] types = new Type[]{typeof((System.Int32 a,System.String b))};
(System.Int32 a,System.String b) returnData=((System.Int32 a,System.String b))client.Invoke(typeof((System.Int32 a,System.String b)),"POST:/xunittestcontroller/test34_reftupleelementnames?",invokeOption,ref parameters,types);
if(parameters!=null)
{
tuple=((System.Int32 a,System.String b))parameters[0];
}
return Task.FromResult<(System.Int32 a,System.String b)>(returnData);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void POST_Test39_Task<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
client.Invoke("POST:/xunittestcontroller/test39_task",invokeOption, null);
}
///<summary>
///无注释信息
///</summary>
public static Task POST_Test39_TaskAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return client.InvokeAsync("POST:/xunittestcontroller/test39_task",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void GET_Test45<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
client.Invoke("GET:/xunittestcontroller/test45?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task GET_Test45Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
return client.InvokeAsync("GET:/xunittestcontroller/test45?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void GET_Test46<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
client.Invoke("GET:/xunittestcontroller/test46?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task GET_Test46Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
return client.InvokeAsync("GET:/xunittestcontroller/test46?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void GET_Test47<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
client.Invoke("GET:/xunittestcontroller/test47?id={0}",invokeOption, parameters);
}
///<summary>
///无注释信息
///</summary>
public static Task GET_Test47Async<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
return client.InvokeAsync("GET:/xunittestcontroller/test47?id={0}",invokeOption, parameters);
}

}
public class ProxyClass1
{
public System.Int32 P1 { get; set; }
public ProxyClass2 P2 { get; set; }
public ProxyClass1 P3 { get; set; }
}

public class Args
{
public System.Int32 P1 { get; set; }
public System.Double P2 { get; set; }
public System.String P3 { get; set; }
}

public class Class04
{
public System.Int32 P1 { get; set; }
public System.String P2 { get; set; }
public System.Int32 P3 { get; set; }
}

public struct StructArgs
{
public System.Int32 P1 { get; set; }
}

public class ProxyClass2
{
public System.Int32 P1 { get; set; }
public ProxyClass3 P2 { get; set; }
}

public enum ProxyType2:System.Int32
{
A=0,
B=1,
C=2,
}

public class ProxyClass3
{
public System.Int32 P1 { get; set; }
public ProxyType1 P2 { get; set; }
public ProxyType2 P3 { get; set; }
public ProxyType3 P4 { get; set; }
}

public class Class01
{
public System.Int32 Age { get; set; }
public System.String Name { get; set; }
public System.Int32? P1 { get; set; }
public System.String? P2 { get; set; }
public (System.String a,System.Int32 b)? P3 { get; set; }
public System.Int32? P4;
public (System.String a,System.Int32 b)? P5;
}

public enum ProxyType3:System.Int64
{
A=0,
B=1,
C=2,
}

public enum ProxyType1:System.SByte
{
A=0,
B=1,
C=2,
}

public class MyClass
{
public System.Int32 P1 { get; set; }
}

}
