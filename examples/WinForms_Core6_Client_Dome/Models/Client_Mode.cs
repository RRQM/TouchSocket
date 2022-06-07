namespace WinForms_Core6_Client_Dome.Models
{
    public class Client_Mode
    {

        public string? ID { get; set; }   //Client ID
        public string? IP { get; set; }  //Client IP
        public int Port { get; set; }  //端口
        public bool Online { get; set; }  // 是否在线
        public bool CanSend { get; set; }
    }
}
