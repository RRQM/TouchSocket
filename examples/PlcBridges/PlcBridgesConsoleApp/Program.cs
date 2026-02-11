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
using TouchSocket.PlcBridges;

namespace PlcBridgesConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            //此功能是Pro版本的功能，如果您有Pro版本的授权，请在此处进行授权。
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        #region Plc桥接基本使用
        //1.初始化PLC桥接服务
        var plcBridge = new PlcBridgeService();

        //2.简单配置
        await plcBridge.SetupAsync(new TouchSocketConfig());

        //3.添加PLC驱动,事实上,您可以在任意时刻继续多个PLC驱动
        // 这里添加一个内存PLC驱动,模拟short类型的PLC数据。
        await plcBridge.AddDriveAsync(new MemoryPlcDrive<short>(CreatePlcDriveOption()));

        //4.启动PLC桥接服务
        await plcBridge.StartAsync();

        //5.在启动后,继续添加另一个PLC驱动,模拟short类型的PLC数据。
        await plcBridge.AddDriveAsync(new MemoryPlcDrive<short>(new PlcDriveOption() { Name = "DeviceB", Start = 10, Count = 10 }));

        //可以再添加一个bool类型的PLC驱动,模拟bool类型的PLC数据。
        await plcBridge.AddDriveAsync(new MemoryPlcDrive<bool>(new PlcDriveOption() { Name = "DeviceC", Start = 0, Count = 10 }));
        #endregion

        //不同类型的PLC驱动地址互不影响。例如：short类型的PLC驱动地址从0开始，bool类型的PLC驱动地址也是从0开始。

        //现在是相当于，我们在Plc桥接服务中，模拟了2个数据类型区。
        //一个是short类型的PLC数据，地址从0到19（2个Drive提供的）。
        //一个是bool类型的PLC数据，地址从0到9（1个Drive提供的）。

        await NormalReadWrite(plcBridge);

        await PlcObjectReadWrite(plcBridge);
        
        await RequestMergeExample(plcBridge);
        
        await MultiDriveExample();
        
        await PerformanceOptimizationExample();
        
        Console.ReadKey();
    }

    #region Plc桥接请求合并优化
    private static async Task RequestMergeExample(PlcBridgeService plcBridge)
    {
        // 配置写入间隙为1，有效窗口500ms
        var driveOption = new PlcDriveOption
        {
            MaxWriteGap = 1,
            WriteGapValidityWindow = TimeSpan.FromMilliseconds(500)
        };

        // 写入操作会自动合并相邻请求
        var plcOperator = plcBridge.CreateOperator<byte>();
        var writeResult = await plcOperator.WriteAsync(
            new WritableValueCollection<byte>(
                new WritableValue<byte>(0, new byte[] {0,1,2,3}),
                new WritableValue<byte>(5, new byte[] {5,6})
            ));
    }
    #endregion

    #region Plc桥接多驱动器协同工作
    private static async Task MultiDriveExample()
    {
        var plcBridge = new PlcBridgeService();
        await plcBridge.SetupAsync(new TouchSocketConfig());
        
        // 添加两个驱动器
        var memoryPlcDrive_1 = new MemoryPlcDrive<short>(
            new PlcDriveOption() { Start = 0, Count = 5 });
        var memoryPlcDrive_2 = new MemoryPlcDrive<short>(
            new PlcDriveOption() { Start = 5, Count = 5 });

        await plcBridge.AddDriveAsync(memoryPlcDrive_1);
        await plcBridge.AddDriveAsync(memoryPlcDrive_2);

        // 写入跨越两个驱动器的数据
        var plcOperator = plcBridge.CreateOperator<short>();
        var writeResult = await plcOperator.WriteAsync(
            new WritableValueCollection<short>(
                new WritableValue<short>(0, new short[] {0,1,2,3,4,5,6,7,8,9})
            ));
    }
    #endregion

    #region Plc桥接性能优化建议
    private static async Task PerformanceOptimizationExample()
    {
        var driveOption = new PlcDriveOption();
        
        // 1. 合理设置MaxGap参数
        // 增大间隙值可提高合并率
        driveOption.MaxReadGap = 20;
        driveOption.MaxWriteGap = 5;
        
        // 2. 使用分组控制执行顺序
        // 相同分组的驱动器串行执行
        driveOption.Group = "CriticalGroup";
        
        // 3. 调整延迟时间平衡实时性与性能
        // 适当增加延迟时间提高合并率
        driveOption.DelayTime = TimeSpan.FromMilliseconds(100);
    }
    #endregion

    private static async Task NormalReadWrite(PlcBridgeService plcBridge)
    {
        #region Plc桥接写入数据
        // 我们现在假设需要写入：
        // DeviceA=>地址为0，长度为5，值为1,2,3,4,5
        // DeviceA=>地址为6，长度为1，值为6
        // DeviceB=>地址为0，长度为2，值为7,8
        // DeviceB=>地址为3，长度为3，值为9,10,11

        //因为DeviceA和DeviceB都已经被映射到short类型的数据区，所以需要创建一个short泛型的Operator。
        var plcShortOperator = plcBridge.CreateOperator<short>();

        var writableValues = new WritableValueCollection<short>()
        {
            new WritableValue<short>(0, new short[]{1,2,3,4,5 }),// DeviceA的起始地址是0
            new WritableValue<short>(6, new short[]{6}),
            new WritableValue<short>(10, new short[]{7,8}),// DeviceB的起始地址是10
            new WritableValue<short>(13, new short[]{9,10,11}),
        };
        var writeResult = await plcShortOperator.WriteAsync(writableValues);
        if (writeResult.IsSuccess)
        {
            Console.WriteLine("短整型数据写入成功");
        }
        else
        {
            Console.WriteLine($"短整型数据写入失败：{writeResult.Message}");
        }
        #endregion

        #region Plc桥接读取数据
        // 我们现在假设需要读取：
        // DeviceA=>地址为0，长度为5
        // DeviceA=>地址为6，长度为1
        // DeviceB=>地址为0，长度为2
        // DeviceB=>地址为3，长度为3

        var readableValues = new ReadableValueCollection<short>()
        {
            new ReadableValue<short>(0, 5), // DeviceA的起始地址是0
            new ReadableValue<short>(6, 1),
            new ReadableValue<short>(10, 2), // DeviceB的起始地址是10
            new ReadableValue<short>(13, 3),
        };

        var readResult = await plcShortOperator.ReadAsync(readableValues);
        if (readResult.IsSuccess)
        {
            Console.WriteLine("短整型数据读取成功");
            foreach (var item in readableValues)
            {
                Console.WriteLine($"地址：{item.Start}，值：{item.Values.ToArray().ToJsonString()}");
            }
        }
        else
        {
            Console.WriteLine($"短整型数据读取失败：{readResult.Message}");
        }
        #endregion

    }

    private static PlcDriveOption CreatePlcDriveOption()
    {
        #region Plc桥接配置示例
        //驱动器配置
        var driveOption = new PlcDriveOption();

        // 驱动名称
        driveOption.Name = "DeviceA";

        // 映射到PLC桥接服务的起始地址
        driveOption.Start = 0;

        // 映射到PLC桥接服务的数量
        driveOption.Count = 10;

        // PLC数据的字节序类型
        driveOption.EndianType = EndianType.Big;

        // 读取地址范围之间的最大间隙，小于间隙的地址范围会被一次性读取操作。
        // 例如：如果批量读取范围0-1，2-3，4-7，最大间隙为10，则会被一次性读取[0-7]。
        driveOption.MaxReadGap = 10;

        // 写入地址范围之间的最大间隙，默认为0。意味着每次写入操作都会单独处理。
        driveOption.MaxWriteGap = 0;

        //但是，MaxWriteGap也可以设置为其他有效值。
        //当MaxWriteGap有效时，应该还要设置WriteGapValidityWindow。
        //意味着在写入操作时，如果在Gap间隙的值，刚刚被读取过，并且操作时间在WriteGapValidityWindow内，
        //则会把刚刚读取到的Gap区间值，再次作为读取值，合并批量写入。这样就避免了Gap间隙的值被0覆盖的问题。
        driveOption.WriteGapValidityWindow = TimeSpan.FromMilliseconds(1000);


        // 驱动器的分组名称。相同分组的驱动器会使用同一个Task，即会串行执行。
        driveOption.Group = "GroupA";

        // 驱动器的轮询延迟时间，默认TimeSpan.Zero。时间越长，批量处理合并的可能性越大。但是，延迟时间过长会导致实时性降低。
        driveOption.DelayTime = TimeSpan.FromMilliseconds(100);
        #endregion

        return driveOption;
    }

    private static async Task PlcObjectReadWrite(PlcBridgeService plcBridge)
    {
        #region Plc桥接使用PlcObject
        var myPlcObject = new MyPlcObject(plcBridge);

        await myPlcObject.SetMyShortValueAsync(10);
        var myShortValue = await myPlcObject.GetMyShortValueAsync();
        Console.WriteLine($"MyShortValue: {myShortValue.Value}");

        var myShortValues = await myPlcObject.GetMyShortValuesAsync();
        Console.WriteLine($"MyShortValues: {myShortValues.Value.ToArray().ToJsonString()}");

        //写入数据
        await myPlcObject.SetMyShortValuesAsync(new short[] { 20, 21 });
        myShortValues = await myPlcObject.GetMyShortValuesAsync();
        Console.WriteLine($"MyShortValues after write: {myShortValues.Value.ToArray().ToJsonString()}");
        #endregion
    }
}

#region Plc桥接定义PlcObject
public partial class MyPlcObject : PlcObject
{
    public MyPlcObject(IPlcBridgeService bridgeService) : base(bridgeService)
    {
    }

    [PlcField<short>(Start = 0)]
    private short m_myShortValue;

    [PlcField<short>(Start = 1, Quantity = 2)]
    private ReadOnlyMemory<short> m_myShortValues;
}
#endregion
