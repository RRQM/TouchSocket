namespace UnityServerConsoleApp_All.TouchServer;

/// <summary>
/// 基础网络服务接口
/// </summary>
public interface BaseTouchServer
{
    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="port"></param>
    Task StartService(int port);
}
