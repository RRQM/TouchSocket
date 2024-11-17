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

namespace XUnitTestProject.Core
{
    
    public class TestAppMessenger
    {
        [Fact]
        public async Task MessageObjectShouldBeOk()
        {
            var appMessenger = new AppMessenger();
            var messageObject = new MessageObject();
            appMessenger.Register(messageObject);
            var add = await appMessenger.SendAsync<int>("Add", 20, 10);
            Assert.Equal(30, add);

            var sub = await appMessenger.SendAsync<int>("Sub", 20, 10);
            Assert.Equal(10, sub);
        }

        [Fact]
        public async Task StaticShouldBeOk()
        {
            var appMessenger = new AppMessenger();
            appMessenger.RegisterStatic<MessageObject>();
            var staticAdd = await appMessenger.SendAsync<int>("StaticAdd", 20, 10);
            Assert.Equal(30, staticAdd);
        }

        //[Fact]
        //public async Task ActionShouldBeOk()
        //{
        //    var appMessenger = new AppMessenger();

        //    appMessenger.Register((int a,int b)=>Task.FromResult(a+b),"Add");

        //    var staticAdd = await appMessenger.SendAsync<int>("Add", 20, 10);
        //    Assert.Equal(30, staticAdd);
        //}
    }

    public class MessageObject : IMessageObject
    {
        [AppMessage]
        public Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        [AppMessage]
        public static Task<int> StaticAdd(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        [AppMessage]
        public Task<int> Sub(int a, int b)
        {
            return Task.FromResult(a - b);
        }
    }
}