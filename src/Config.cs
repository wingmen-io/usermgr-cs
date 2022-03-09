using dotenv.net;

namespace Aserto.UserManager
{
    public record Config
    {
        public Aserto.Config Aserto { get; init; }

        public Okta.Config Okta { get; init; }

        public Config()
        {
            var cfg = DotEnv.Fluent()
                .WithExceptions()
                .WithEnvFiles()
                .Read();

            Aserto = new Aserto.Config
            {
                TenantID = cfg["ASERTO_TENANT_ID"],
                AuthorizerAddr = cfg["ASERTO_AUTHORIZER_ADDR"],
                AuthorizerAPIKey = cfg["ASERTO_AUTHORIZER_API_KEY"]
            };

            Okta = new Okta.Config
            {
                Domain = cfg["OKTA_DOMAIN"],
                Token = cfg["OKTA_API_TOKEN"]
            };
        }
    }
}