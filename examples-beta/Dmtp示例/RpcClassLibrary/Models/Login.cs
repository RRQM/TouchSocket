using System;
using System.Collections.Generic;
using System.Text;

namespace RpcClassLibrary.Models
{
    public class LoginRequest:RequestBase
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse : ResponseBase
    {
    }
}
