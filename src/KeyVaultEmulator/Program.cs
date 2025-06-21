using AzureKeyVaultEmulator.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Elasticsearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AzureKeyVaultEmulator
{
    public class Program
    {
        static readonly LoggerProviderCollection Providers = new LoggerProviderCollection();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var config = context.Configuration;
                        var cert = X509CertificateLoader.LoadPkcs12(
                            File.ReadAllBytes("./certificate.pfx"),
                            config["ServerPFX"]);

                        options.ConfigureEndpointDefaults(listenOptions =>
                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                SslProtocols =  System.Security.Authentication.SslProtocols.Tls12,
                                ServerCertificate = cert
                            }));
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((builderContext, config) =>
                {
                    config
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.WithProcessId()
                        .Enrich.WithProcessName()
                        .Enrich.WithThreadId()
                        .Enrich.WithThreadName()
                        .Enrich.WithUserName()
                        .WriteTo.Providers(Providers)
                        .WriteTo.Console(new ElasticsearchJsonFormatter());
                });
    }
}
