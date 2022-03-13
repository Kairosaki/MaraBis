using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

namespace Marabis
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;

        private ulong _channelId;
        public ulong ChannelId { get => _channelId; private set => _channelId = value; }

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += HandleMessage;
            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

            if (!command.IsSpecified || result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"Error: {result}");
        }

        private async Task HandleMessage(SocketMessage incomingMessage)
        {
            if (incomingMessage is not SocketUserMessage message) return;

            if (incomingMessage.Source != MessageSource.User) return;

            if (incomingMessage.Channel.Id != _config.GetValue<ulong>("ChannelId")) return;

            int argPos = 0;

            if (!message.HasStringPrefix(_config["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

            if (message.Author.IsBot) return;

            var context = new SocketCommandContext(Client, message);

            await _commandService.ExecuteAsync(context, argPos, _provider);

        }       
    }
}
