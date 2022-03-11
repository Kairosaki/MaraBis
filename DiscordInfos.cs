using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MaraBis
{
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

        public async Task InstallCommandsAsync()
        {
            Client.MessageReceived += HandleCommandAsync;
            await Commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(Client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(Client, message);

            await Commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }

        public async Task RunBot() 
        {
            Client = new DiscordSocketClient();

            Commands = new CommandService();

            Config = new DiscordSocketConfig()
            {

                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
            };

            Client.Log += Log;

            Client.Ready += () =>
            {
                Console.WriteLine("Connection réussi");
                return Task.CompletedTask;
            };

            await InstallCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken", EnvironmentVariableTarget.User));

            await Client.StartAsync();

            await Task.Delay(-1);

        }
    }
}
