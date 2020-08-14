using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ray.EssayNotes.Di.InitDemo
{
    public class Program
    {
        public static IServiceProvider InitialServiceProvider;

        public static void Main(string[] args)
        {
            //1.Host������
            IHostBuilder builder = CreateHostBuilder(args);

            //2.����Host����ʱ��ʵ����Startup��������ConfigureServices����
            IHost host = builder.Build();
            Console.WriteLine($"IHost.Services��{host.Services.GetHashCode()}");

            //3.���У���ʱ�����Startup.Configure����
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
            return builder;
        }
    }
}
