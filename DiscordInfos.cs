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
        private DiscordSocketClient _client;
        private CommandService _commands;
        private DiscordSocketConfig _config;

        public DiscordSocketClient Client { get => _client; private set => _client = value; }
        public CommandService Commands { get => _commands; set => _commands = value; }
        public DiscordSocketConfig Config { get => _config; private set => _config = value; }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
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
