using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;

namespace Marabis.Services
{
    public class InteractionHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly InteractionService _interactionService;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _config;

        private readonly HostBuilderContext _hostContext;

        public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService, IHostEnvironment environment, IConfiguration config, HostBuilderContext hostContext) : base(client, logger)
        {
            _provider = provider;
            _interactionService = interactionService;
            _environment = environment;
            _config = config;
            _hostContext = hostContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Client.InteractionCreated += HandleInteraction;

            _interactionService.SlashCommandExecuted += SlashCommandExecuted;
            _interactionService.ContextCommandExecuted += ContextCommandExecuted;
            _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            await Client.WaitForReadyAsync(stoppingToken);

            // _environment to check if development or production
            // if else
            await _interactionService.RegisterCommandsGloballyAsync();
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo command, IInteractionContext context, IResult result)
        {

            ExceptionHandler(context, result);

            return Task.CompletedTask;

        }

        private Task ContextCommandExecuted(ContextCommandInfo command, IInteractionContext context, IResult result)
        {
            ExceptionHandler(context, result);

            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            ExceptionHandler(context, result);

            return Task.CompletedTask;
        }

        private async Task HandleInteraction(SocketInteraction socketInteraction)
        {
            ulong channelId = Convert.ToUInt64(_hostContext.Configuration["ChannelId"]);

            if (socketInteraction.Channel.Id != channelId)
            {
                Logger.LogInformation($"User {socketInteraction.User.Username} tried to use a command on the wrong channel");
                await socketInteraction.RespondAsync($"wrong channel, please go to <#{channelId}>");
                return;
            } else
            {
                try
                {
                    var context = new SocketInteractionContext(Client, socketInteraction);
                    await _interactionService.ExecuteCommandAsync(context, _provider);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

                    if (socketInteraction.Type == InteractionType.ApplicationCommand)
                    {
                        var msg = await socketInteraction.GetOriginalResponseAsync();
                        await msg.DeleteAsync();
                    }
                }
            }           
        }


        private static void ExceptionHandler(IInteractionContext interactionContext, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        interactionContext.Channel.SendMessageAsync("Les conditions requises pour utiliser cette commande n'ont pas été réunies !");
                        break;
                    case InteractionCommandError.UnknownCommand:
                        interactionContext.Channel.SendMessageAsync("Commande inconnue !");
                        break;
                    case InteractionCommandError.BadArgs:
                        interactionContext.Channel.SendMessageAsync("Arguments invalides !");
                        break;
                    case InteractionCommandError.Exception:
                        interactionContext.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        interactionContext.Channel.SendMessageAsync($"Echec de l'exécution de la commande {result.ErrorReason}");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
