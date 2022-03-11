using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MaraBis.Services;
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
        public DiscordSocketClient Client { get; private set; }
        public CommandService Commands { get; set; }
        public DiscordSocketConfig Config { get; private set; }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task RunBot() 
        {          

            Commands = new CommandService();

            Config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences
            };

            Client = new DiscordSocketClient(Config);

            var cmdHandler = new CommandHandler(Client, Commands);

            Client.Log += Log;

            Client.Ready += () =>
            {
                Console.WriteLine("Connection réussi");
                return Task.CompletedTask;
            };

            await cmdHandler.InstallCommandsAsync();            

            await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken", EnvironmentVariableTarget.User));

            await Client.StartAsync();

            await Task.Delay(-1);

        }
    }
}
