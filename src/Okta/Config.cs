namespace Aserto.UserManager.Okta
{
    public record Config
    {
        public string Domain { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
    }
}
