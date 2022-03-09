using System.CommandLine;

namespace Aserto.UserManager
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var userOption = new Option<string>(
                "--user-id",
                getDefaultValue: () => string.Empty,
                description: "user identifier (email or provider ID)");

            var rootCommand = new RootCommand
            {
                userOption
            };

            rootCommand.Description = "User Manager";

            rootCommand.SetHandler(async (string userID) => {
                var config = new Config();
                var producer = new Okta.Producer(config.Okta);
                var subscriber = new Aserto.Subscriber(config.Aserto);

                var asertoUsers = Okta.Producer.Transform(
                    (string.IsNullOrEmpty(userID) ? producer.Users() : producer.UserByID(userID)));

                await foreach (var asertoUser in asertoUsers)
                {
                    asertoUser.Dump();
                }

                var result = await subscriber.LoadUsers(asertoUsers);

                Console.WriteLine("received {0}", result.Received);
                Console.WriteLine("created  {0}", result.Created);
                Console.WriteLine("updated  {0}", result.Updated);
                Console.WriteLine("deleted  {0}", result.Deleted);
                Console.WriteLine("errors   {0}", result.Errors);

            }, userOption);

            await rootCommand.InvokeAsync(args);
        }
    }
}
