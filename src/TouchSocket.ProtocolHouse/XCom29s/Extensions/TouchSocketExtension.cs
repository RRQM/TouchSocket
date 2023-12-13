using Microsoft.Extensions.DependencyInjection;

namespace TouchSocket.ProtocolHouse;

public static class TouchSocketExtension
{
    public static void AddTouchSocket(this IServiceCollection services)
    {
        services.AddSingleton<CommServiceSocket<XCom29sPlugin>>();
        services.AddSingleton<ICommServiceSocket, CommServiceSocket<XCom29sPlugin>>();

        services.AddSingleton<CommClientSocket<XCom29sPlugin>>();
        services.AddSingleton<ICommClientSocket, CommClientSocket<XCom29sPlugin>>();
    }
}