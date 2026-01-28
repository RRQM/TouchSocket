using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TouchSocket.Rpc;

namespace WebApiAuthenticationConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = await CreateWebApiService();
            await service.StartAsync();
            Console.WriteLine("WebApi服务已启动: http://localhost:7789");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static async Task<HttpService> CreateWebApiService()
        {
            #region WebApi基于插件的鉴权
            var service = new HttpService();
            await service.SetupAsync(new TouchSocketConfig()
                .SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddRpcStore(store =>
                    {
                        store.RegisterServer<AuthorizedController>();
                        store.RegisterServer<RoleBasedController>();
                        store.RegisterServer<LoginController>();
                    });
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<AuthPlugin>(); // 添加鉴权插件
                    a.UseWebApi();
                    a.UseDefaultHttpServicePlugin();
                }));
            return service;
            #endregion
        }

        #region WebApi基于特性的授权
        [Router("/[api]/[action]")]
        [WebApiAuthorize] // 控制器级别授权
        public class AuthorizedController : SingletonRpcServer
        {
            [Router("[api]/[action]")]
            [WebApi(Method = HttpMethodType.Get)]
            public string GetUserInfo(IWebApiCallContext callContext)
            {
                // 假设从认证上下文中获取用户信息
                if (callContext.Caller is IHttpSessionClient client)
                {
                    return $"User ID: {client.Id}";
                }
                return "Unknown user";
            }

            [Router("[api]/[action]")]
            [AllowAnonymous] // 允许匿名访问
            [WebApi(Method = HttpMethodType.Get)]
            public string PublicData()
            {
                return "This is public data";
            }
        }
        #endregion

        #region WebApiJWT鉴权实现
        public static string GenerateJwtToken(string userId, string userName)
        {
            // 假逻辑：生成JWT Token
            var payload = new
            {
                sub = userId,
                name = userName,
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            };
            
            // 实际应用中应使用真实的JWT库进行签名
            var fakeToken = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(payload)))}.signature";
            return fakeToken;
        }

        public static bool ValidateJwtToken(string token, out string userId)
        {
            userId = null;
            
            // 假逻辑：验证JWT Token
            if (string.IsNullOrEmpty(token) || !token.StartsWith("eyJ"))
            {
                return false;
            }
            
            try
            {
                // 实际应用中应验证签名、过期时间等
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    return false;
                }
                
                var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                var payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(payloadJson);
                
                userId = payload.GetProperty("sub").GetString();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region WebApi基于角色的授权
        [Router("/[api]/[action]")]
        public class RoleBasedController : SingletonRpcServer
        {
            [Router("[api]/[action]")]
            [WebApiAuthorize(Roles = "Admin")] // 仅管理员可访问
            [WebApi(Method = HttpMethodType.Get)]
            public string AdminOnly()
            {
                return "This is admin only content";
            }

            [Router("[api]/[action]")]
            [WebApiAuthorize(Roles = "Admin,User")] // 管理员或普通用户可访问
            [WebApi(Method = HttpMethodType.Get)]
            public string UserData()
            {
                return "User data content";
            }

            [Router("[api]/[action]")]
            [WebApiAuthorize(Policy = "RequireAdminAndVerified")] // 基于策略的授权
            [WebApi(Method = HttpMethodType.Get)]
            public string SpecialResource()
            {
                return "Special resource for verified admins";
            }
        }
        #endregion

        #region WebApi登录获取Token
        [Router("/[api]/[action]")]
        public class LoginController : SingletonRpcServer
        {
            [Router("[api]/[action]")]
            [AllowAnonymous]
            [WebApi(Method = HttpMethodType.Post)]
            public LoginResponse Login(LoginRequest request)
            {
                // 假逻辑：验证用户名和密码
                if (request.Username == "admin" && request.Password == "password")
                {
                    var token = GenerateJwtToken("user123", request.Username);
                    return new LoginResponse
                    {
                        Success = true,
                        Token = token,
                        Message = "Login successful"
                    };
                }
                
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public bool Success { get; set; }
            public string? Token { get; set; }
            public string Message { get; set; } = string.Empty;
        }
        #endregion

        #region WebApi自定义授权策略
        // 自定义授权策略示例
        // 在实际应用中，可以实现自己的授权逻辑
        public class CustomAuthorizationPolicy
        {
            private static readonly DependencyProperty<string[]> RolesProperty = new("Roles", Array.Empty<string>());
            private static readonly DependencyProperty<bool> VerifiedProperty = new("Verified", false);

            public Task<bool> AuthorizeAsync(IHttpSessionClient client, string policy)
            {
                // 假逻辑：自定义授权策略
                if (policy == "RequireAdminAndVerified")
                {
                    // 从客户端上下文中获取用户信息
                    var roles = client.GetValue(RolesProperty);
                    var isVerified = client.GetValue(VerifiedProperty);
                    
                    var isAdmin = roles?.Contains("Admin") ?? false;
                    return Task.FromResult(isAdmin && isVerified);
                }
                
                return Task.FromResult(false);
            }
        }
        #endregion
    }

    #region WebApi鉴权插件实现
    public class AuthPlugin : PluginBase, IHttpPlugin
    {
        private static readonly DependencyProperty<string> UserIdProperty = new("UserId", string.Empty);

        public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            var request = e.Context.Request;
            var token = request.Headers.Get("Authorization");

            // 假逻辑：验证Token
            if (string.IsNullOrEmpty(token) || !IsValidToken(token))
            {
                // 未授权，返回401
                e.Context.Response
                    .SetStatus(401, "Unauthorized")
                    .SetContent("Unauthorized access");
                await e.Context.Response.AnswerAsync();
                e.Handled = true; // 阻止继续处理
                return;
            }

            // 假逻辑：从Token中提取用户信息并存储到上下文
            var userId = ExtractUserIdFromToken(token);
            client.SetValue(UserIdProperty, userId);

            await e.InvokeNext();
        }

        private bool IsValidToken(string token)
        {
            // 假逻辑：验证Token
            // 实际应用中应验证JWT签名、过期时间等
            return !string.IsNullOrEmpty(token) && token.StartsWith("Bearer ");
        }

        private string ExtractUserIdFromToken(string token)
        {
            // 假逻辑：从Token中提取用户ID
            // 实际应用中应解析JWT
            return "user123";
        }
    }
    #endregion

    #region WebApi授权特性
    // WebApiAuthorize特性用于标记需要授权的控制器或方法
    public class WebApiAuthorizeAttribute : Attribute
    {
        public string? Roles { get; set; }
        public string? Policy { get; set; }
    }

    // AllowAnonymous特性用于标记允许匿名访问的方法
    public class AllowAnonymousAttribute : Attribute
    {
    }
    #endregion

    #region WebApi授权策略接口
    // 授权策略接口
    // 在实际应用中，可以定义统一的授权策略接口
    public interface IAuthorizationPolicy
    {
        Task<bool> AuthorizeAsync(IHttpSessionClient client, string policy);
    }
    #endregion
}
