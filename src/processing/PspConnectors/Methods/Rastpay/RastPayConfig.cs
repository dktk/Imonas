namespace PspConnectors.Methods.Rastpay
{
    public sealed class RastPayConfig
    {
        public string BaseUrl { get; set; }
        public string[] SecretKeys { get; set; }
        public string AuthToken { get; set; }
    }
}