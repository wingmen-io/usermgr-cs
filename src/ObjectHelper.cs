using System.Text.Json;

namespace Aserto.UserManager
{
    internal static class ObjectHelper
    {
        public static void Dump<T>(this T x)
        {
            Console.WriteLine(JsonSerializer.Serialize(x, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
