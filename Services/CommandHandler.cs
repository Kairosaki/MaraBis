using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MaraBis.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }


        private Task ComponentCommandExecuted(ComponentCommandInfo ctxCommandInfo, Discord.IInteractionContext interactionContext, IResult result)
        {
            ExceptionHandler(interactionContext, result);

            return Task.CompletedTask;
        }

        

        private Task ContextCommandExecuted(ContextCommandInfo ctxCommandInfo, Discord.IInteractionContext interactionContext, IResult result)
        {
            ExceptionHandler(interactionContext, result);

            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo ctxCommandInfo, Discord.IInteractionContext interactionContext, IResult result)
        {
            ExceptionHandler(interactionContext, result);           

            return Task.CompletedTask;
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                // create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, interaction);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // if a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
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
