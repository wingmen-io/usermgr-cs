using Okta.Sdk;
using Okta.Sdk.Configuration;

namespace Aserto.UserManager.Okta
{
    internal class Producer
    {
        private readonly OktaClient client;

        public Producer(Config config)
        {
            client = new OktaClient(new OktaClientConfiguration
            {
                OktaDomain = config.Domain,
                Token = config.Token
            });
        }

        public async IAsyncEnumerable<IUser> Users()
        {
            var users = await client.Users.ToArrayAsync();
            foreach (var user in users)
            {
                yield return user;
            }
            
            yield break;
        }

        public async IAsyncEnumerable<IUser> UserByID(string id)
        {
            var user = await client.Users.GetUserAsync(id);
            yield return user;
        }

        public async IAsyncEnumerable<IUser> UserByEmail(string email)
        {
            var user = await client.Users.GetUserAsync(email);
            yield return user;
        }

        public static async IAsyncEnumerable<API.V1.User> Transform(IAsyncEnumerable<IUser> users)
        {
            await foreach (var user in users)
            {
                var asertoUser = transform(user);
                yield return asertoUser;
            }
        }

        private static API.V1.User transform(IUser user)
        {
            var asertoUser = new API.V1.User
            {
                Id = user.Id,
                Enabled = user.Status == UserStatus.Active,
                Deleted = user.Status == UserStatus.Deprovisioned,
                Email = user.Profile.Email
            };

            if (!string.IsNullOrEmpty(user.Profile.DisplayName))
            {
                asertoUser.DisplayName = user.Profile.DisplayName;
            }

            asertoUser.Identities.Add(user.Id, new API.V1.IdentitySource
            {
                Kind = API.V1.IdentityKind.Pid,
                Provider = "okta",
                Verified = true
            });

            asertoUser.Identities.Add(user.Profile.Email, new API.V1.IdentitySource
            {
                Kind = API.V1.IdentityKind.Email,
                Provider = "okta",
                Verified = true
            });

            return asertoUser;
        }
    }
}