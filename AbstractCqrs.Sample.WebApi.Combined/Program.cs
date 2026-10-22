using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AbstractCqrs.Sample.WebApi.Combined
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel(options =>
                                    {
                                        options.Listen(IPAddress.Loopback, 5000);
                                    });
                });
        }
    }
}