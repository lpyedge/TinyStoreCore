using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TinyStore.Site
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                    //注意相对路径用/分隔
                    config.AddJsonFile("App_Data/Config.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(option =>
                        {
                            option.ConfigureHttpsDefaults((httpsConfig) =>
                            {
                                var certPemFile =
                                    new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "App_Data/SSL/cert.pem");
                                var privateKeyPemFile =
                                    new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "App_Data/SSL/prrvate.key");
                                if (certPemFile.Exists && privateKeyPemFile.Exists)
                                {
                                    httpsConfig.ServerCertificate = X509Certificate2.CreateFromPemFile(certPemFile.FullName,privateKeyPemFile.FullName);
                                    httpsConfig.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                                }
                            });
                        })
                        .UseIIS()
                        .UseStartup<Startup>();
                });
    }
}