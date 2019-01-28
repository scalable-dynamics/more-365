using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace more365.AppProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<More365MiddlewareStartup>()
                   .Build()
                   .Run();
        }
    }
}