namespace Think4.Services
{
    public class JwtConfiguration
    {
        public string Secret { get; set; }
        public int TokenExpirationInHours { get; set; }
        public int SessionTimeoutInMinutes { get; set; }
        public string ManagerSecretKey { get; set; }
    }
}