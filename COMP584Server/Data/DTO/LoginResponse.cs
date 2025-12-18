namespace COMP584Server.Data.DTO
{
    public class LoginResponse
    {
        public bool Success {get; set; }
        public string Message {get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty ;
        //token expiration so clients can handle session timing and reauthentication
        public DateTime Expiration { get; set; }

    }
}
