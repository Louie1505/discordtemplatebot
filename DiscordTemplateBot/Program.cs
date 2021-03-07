using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordTemplateBot
{
    class Program
    {
        public static IConfigurationRoot configuration;
        public static DiscordSocketClient Client;
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(
              options => options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                  .Configure<EventLogSettings>(config =>
                  {
                      config.LogName = "Application";
                      config.SourceName = "DiscordTemplateBot";
                  });
                // Add access to generic IConfigurationRoot
                services.AddSingleton<IConfigurationRoot>(configuration);
            }).UseWindowsService();
        }
    }
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                new Bot().StartAsync().GetAwaiter().GetResult();
            }
        }
    }
    public class Bot
    {
        CommandHandler handler;

        public async Task StartAsync()
        {
            Program.Client = new DiscordSocketClient();
            Program.Client.Log += Log;

            var token = Program.configuration["Token"];

            await Program.Client.LoginAsync(TokenType.Bot, token);
            await Program.Client.StartAsync();

            this.handler = new CommandHandler(Program.Client, new Discord.Commands.CommandService());
            await handler.InstallCommandsAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}