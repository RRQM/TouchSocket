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
    
    public class TestMethod
    {
        [Fact]
        public void Method1ShouleBeOk()
        {
            var method = this.GetMethod("Method1");
            Assert.True(method.HasReturn);
            Assert.True(!method.HasByRef);
            Assert.True(method.ReturnType == TouchSocketCoreUtility.intType);
            Assert.True(method.TaskType == TaskReturnType.None);
            Assert.True((int)method.Invoke(new MyMethod(), 10) == 10);
        }

        [Fact]
        public void Method2ShouleBeOk()
        {
            var method = this.GetMethod("Method2");
            Assert.True(method.HasReturn);
            Assert.True(method.HasByRef);
            Assert.True(method.ReturnType == TouchSocketCoreUtility.intType);
            Assert.True(method.TaskType == TaskReturnType.None);

            var objs = new object[] { 10 };
            Assert.True((int)method.Invoke(new MyMethod(), objs) == 11);
            Assert.True((int)objs[0] == 11);
        }

        [Fact]
        public async Task Method3ShouleBeOk()
        {
            var method = this.GetMethod("Method3");
            Assert.True(!method.HasReturn);
            Assert.True(!method.HasByRef);
            Assert.True(method.ReturnType == null);
            Assert.True(method.TaskType == TaskReturnType.Task);
            await method.InvokeAsync(new MyMethod());
        }

        [Fact]
        public async Task Method4ShouleBeOk()
        {
            var method = this.GetMethod("Method4");
            Assert.True(method.HasReturn);
            Assert.True(method.HasByRef);
            Assert.True(method.ReturnType == typeof(int));
            Assert.True(method.TaskType == TaskReturnType.TaskObject);

            var objs = new object[] { 10 };
            Assert.True((int)await method.InvokeObjectAsync(new MyMethod(), objs) == 11);
            Assert.True((int)objs[0] == 11);

            Assert.True(await method.InvokeAsync<int>(new MyMethod(), objs) == 12);
            Assert.True((int)objs[0] == 12);
        }

        [Fact]
        public async Task Method5ShouleBeOk()
        {
            var method = this.GetMethod("Method5");
            Assert.True(method.HasReturn);
            Assert.True(!method.HasByRef);
            Assert.True(method.ReturnType == typeof(int));
            Assert.True(method.TaskType == TaskReturnType.TaskObject);

            var objs = new object[] { 10 };
            Assert.True((int)await method.InvokeObjectAsync(new MyMethod(), objs) == 11);
            Assert.True(await method.InvokeAsync<int>(new MyMethod(), objs) == 11);
        }

        private Method GetMethod(string name)
        {
            return new Method(typeof(MyMethod).GetMethod(name));
        }
    }

    public class MyMethod
    {
        public int Method1(int a)
        {
            return a;
        }

        public int Method2(ref int a)
        {
            a++;
            return a;
        }

        public Task Method3()
        {
            return Task.CompletedTask;
        }

        public Task<int> Method4(ref int a)
        {
            a++;
            return Task.FromResult(a);
        }

        public Task<int> Method5(int a)
        {
            return Task.Run(() =>
            {
                a++;
                return a;
            });
        }
    }
}