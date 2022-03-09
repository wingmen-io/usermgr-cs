namespace Aserto.UserManager.Aserto
{
    public record Config
    {
        public string AuthorizerAddr { get; init; } = string.Empty;
        public string AuthorizerAPIKey { get; init; } = string.Empty;
        public string TenantID { get; init; } = string.Empty;
    }
}
