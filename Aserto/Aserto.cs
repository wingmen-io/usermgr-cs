using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;

using static Aserto.Authorizer.Directory.V1.Directory;

namespace Aserto.UserManager.Aserto
{
    public record Config
    {
        public string AuthorizerAddr { get; init; } = string.Empty;
        public string AuthorizerAPIKey { get; init; } = string.Empty;
        public string TenantID { get; init; } = string.Empty;
    }

    class Subscriber
    {
        DirectoryClient client;
        GrpcChannel channel;
        Grpc.Core.Metadata headers;

        public Subscriber(Config config)
        {
            var httpHandler = new HttpClientHandler();
            // httpHandler.ServerCertificateCustomValidationCallback = Insecure;

            this.channel = GrpcChannel.ForAddress(config.AuthorizerAddr, new GrpcChannelOptions
            {
                Credentials = Grpc.Core.ChannelCredentials.SecureSsl,
                HttpHandler = httpHandler
            });

            this.headers = new()
            {
                { "aserto-tenant-id", config.TenantID },
                { "authorization", "basic " + config.AuthorizerAPIKey }
            };

            this.client = new DirectoryClient(channel);
        }

        public async Task<Authorizer.Directory.V1.LoadUsersResponse> LoadUsers(IAsyncEnumerable<API.V1.User> users)
        {
            using (var call = this.client.LoadUsers(this.headers))
            {
                await foreach (var user in users)
                {
                    var loadUserReq = new Authorizer.Directory.V1.LoadUsersRequest
                    {
                        User = user
                    };

                    await call.RequestStream.WriteAsync(loadUserReq);
                }
                await call.RequestStream.CompleteAsync();

                var summary = await call.ResponseAsync;
                return summary;
            }
        }

        public string GetIdentity(string identity)
        {
            var getIdentityReq = new Authorizer.Directory.V1.GetIdentityRequest{
                Identity = identity
            };
        
            try
            {
                var getIdentityResult = this.client.GetIdentity(getIdentityReq, this.headers);
                return getIdentityResult.Id;
            }
            catch (Exception) // TODO explicit check for not found 
            {
                return "";
            }
        }

        public  API.V1.User GetUser(string id)
        {
            var getUserReq = new Authorizer.Directory.V1.GetUserRequest();
            getUserReq.Id = id;

            try 
            {
                var getUserResult = client.GetUser(getUserReq, headers);
                return getUserResult.Result;
            }
            catch (Exception)
            {
                return new API.V1.User{};
            }
        }

        private static bool Insecure(HttpRequestMessage requestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslErrors)
        {
            Console.WriteLine("!!! insecure connection, TLS verification skipped !!!");
            return true;
        }
    }

}