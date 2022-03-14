using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Hosting;
using Victoria;
using Marabis.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureDiscordHost((context, config) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.All,
            MessageCacheSize = 200
        };

        config.Token = context.Configuration["token"];
    })
    .UseCommandService((context, config) =>
    {
        config.DefaultRunMode = RunMode.Async;
        config.CaseSensitiveCommands = false;
    })
    .UseInteractionService((context, config) =>
    {
        config.LogLevel = LogSeverity.Info;
        config.UseCompiledLambda = true;
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<CommandHandler>();
        services.AddHostedService<InteractionHandler>();
        services.AddHostedService<BotStatusService>();
        services.AddHostedService<LongRunningService>();
        //services.AddHostedService<AudioService>();
        services.AddSingleton<AudioService>();
        services.AddLavaNode(x => x.SelfDeaf = false);
    })
    .Build();

await host.RunAsync();
