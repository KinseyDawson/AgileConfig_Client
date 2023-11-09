using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using AgileConfig.Client;

namespace AgileConfigMVCSampleNET5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseAgileConfig((ConfigClientOptions op) =>
                {
                    op.Name = "xxx";
                    op.Tag = "NET5";
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
