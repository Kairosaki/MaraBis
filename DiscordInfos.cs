using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MaraBis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MaraBis
{
    /// <summary>
    /* idée en vrac:
     * rgb to hexa
     * ascii char
     * crypter un message
     * générer un ssh      
    */
    /// </summary>
    public class DiscordInfos
    {
        //public readonly ulong _guildId = 918042511245733939;

        private readonly IConfiguration _config;

        private readonly ulong _guildId;
        public DiscordSocketClient Client { get; private set; }
        public InteractionService Commands { get; set; }
        public DiscordSocketConfig Config { get; private set; }
        public DiscordInfos()
        {
            var _builder = new ConfigurationBuilder()
                   .SetBasePath(AppContext.BaseDirectory)
                   .AddJsonFile(path: "config.json");

            _config = _builder.Build();
            _guildId = ulong.Parse(_config["ServeurTestId"]);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task RunBot() 
        {

/*            Config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences
            };*/

            using (var services = ConfigureServices())
            {
                Client = services.GetRequiredService<DiscordSocketClient>();
                Commands = services.GetRequiredService<InteractionService>();
                //Config = services.GetRequiredService<DiscordSocketConfig>();

                Client.Log += Log;
                Commands.Log += Log;
                Client.Ready += ReadyAsync;  
          

                await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken", EnvironmentVariableTarget.User));

                await Client.StartAsync();

                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }

        }

        private async Task ReadyAsync()
        {
            // set true for all discords
            await Commands.RegisterCommandsToGuildAsync(_guildId);

            Console.WriteLine($"Connected as -> [{Client.CurrentUser}] :)");
        }

        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using Marabis.Services;
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new DiscordSocketConfig())
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }
    }
}
