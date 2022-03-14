using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

namespace Marabis.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;

        private readonly HostBuilderContext _hostContext;

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, HostBuilderContext hostContext) : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
            _hostContext = hostContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += HandleMessage;
            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
                return;

            Logger.LogInformation($"User {context.User} used command {command.Value.Name} with success");

            await context.Channel.SendMessageAsync($"Error: {result}");
        }

        private async Task HandleMessage(SocketMessage incomingMessage)
        {
            if (incomingMessage is not SocketUserMessage message) return;

            if (incomingMessage.Source != MessageSource.User) return;

            if (incomingMessage.Channel.Id != Convert.ToUInt64(_hostContext.Configuration["ChannelId"]))
            {
                Logger.LogInformation($"User {incomingMessage.Author.Username} tried to use a command on the wrong channel");
                return;
            }

            int argPos = 0;

            if (!message.HasStringPrefix(_config["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

            if (message.Author.IsBot) return;

            var context = new SocketCommandContext(Client, message);

            await _commandService.ExecuteAsync(context, argPos, _provider);

        }       
    }
}
