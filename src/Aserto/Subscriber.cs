using Grpc.Net.Client;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static Aserto.Authorizer.Directory.V1.Directory;

namespace Aserto.UserManager.Aserto
{
    internal class Subscriber
    {
        private readonly DirectoryClient client;
        private readonly GrpcChannel channel;
        private readonly Grpc.Core.Metadata headers;

        public Subscriber(Config config)
        {
            var httpHandler = new HttpClientHandler();
            // httpHandler.ServerCertificateCustomValidationCallback = Insecure;

            channel = GrpcChannel.ForAddress(config.AuthorizerAddr, new GrpcChannelOptions
            {
                Credentials = Grpc.Core.ChannelCredentials.SecureSsl,
                HttpHandler = httpHandler
            });

            headers = new()
            {
                { "aserto-tenant-id", config.TenantID },
                { "authorization", "basic " + config.AuthorizerAPIKey }
            };

            client = new DirectoryClient(channel);
        }

        public async Task<Authorizer.Directory.V1.LoadUsersResponse> LoadUsers(IAsyncEnumerable<API.V1.User> users)
        {
            using (var call = client.LoadUsers(headers))
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
            var getIdentityReq = new Authorizer.Directory.V1.GetIdentityRequest
            {
                Identity = identity
            };

            try
            {
                var getIdentityResult = client.GetIdentity(getIdentityReq, headers);
                return getIdentityResult.Id;
            }
            catch (Exception) // TODO explicit check for not found 
            {
                return string.Empty;
            }
        }

        public API.V1.User GetUser(string id)
        {
            var getUserReq = new Authorizer.Directory.V1.GetUserRequest
            {
                Id = id
            };

            try
            {
                var getUserResult = client.GetUser(getUserReq, headers);
                return getUserResult.Result;
            }
            catch (Exception)
            {
                return new API.V1.User { };
            }
        }

        private static bool Insecure(HttpRequestMessage requestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslErrors)
        {
            Console.WriteLine("!!! insecure connection, TLS verification skipped !!!");
            return true;
        }
    }

}
