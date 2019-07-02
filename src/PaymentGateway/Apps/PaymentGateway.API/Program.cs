using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureKestrel((context, options) =>
                {
                    // Set properties and call methods on options
                    //options.Limits.MaxConcurrentConnections = 1000;
                    //options.Limits.MaxConcurrentUpgradedConnections = 1000;
                })
                
                .Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
