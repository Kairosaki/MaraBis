using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;

namespace Marabis.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<CommandsModule> _logger;
        private readonly IHost _host;

        public CommandsModule(IHost host, ILogger<CommandsModule> logger)
        {
            _host = host;
            _logger = logger;
        }

        [Command("Hello")]
        public async Task Greeting()
        {
            _logger.LogInformation("User {userName} used the Hello command!", Context.User.Username);
            await ReplyAsync($"Hello {Context.User.Mention}");
        }

        [Command("shutdown")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Stop()
        {
            ReplyAsync("Bye bye");
            _ = _host.StopAsync();
            return Task.CompletedTask;
        }

        [Command("Clean")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Clean(int amount = 99)
        {
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount + 1).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

            var count = filteredMessages.Count();

            if (count == 0) await ReplyAsync("Can't delete messages older than 2 weeks");
            else
            {
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);
                await ReplyAsync($"Nombres de messages supprimés : {count}");
                await Task.Delay(3000);
            }
        }

        [Command("Pedro")]
        [Summary("Phrases cultesde pedro")]
        public async Task PhrasesCultes()
        {
            List<string> phrases =
            new()
            {
                "Qui es tu toi?",
                "Afk bio",
                "Oui",
                "J'essaye d'être homogène et lent",
                "Rachid es tu la?",
                "Je me propose",
                "Je suis où?",
                "Vous le voyez avec vos yeux vus",
                "Vous êtes à Saint-Tropez ?",
                "Rubik's cube ... Vous avez connu cela?",
                "... il grêle chez moi.",
                "euh...euh...ouais",
                "On a mangé la viande, là on va attaquer l'os",
                "e e e e e e eeee e e e eee",
                "Qui es-tu ?",
                "oui, oui, oui, peut-être, oui",
                "euhhhh euhhhh euhh... Qui es tu ?",
                "Je n'ai pas de réponse... Vous êtes ébahis ?",
                "Fou else",
                "Sungle thon",
                "Le coute"
            };

            Random rnd = new Random();
            int pick = rnd.Next(0, phrases.Count);

            await ReplyAsync(phrases.ElementAt(pick));
        }

        [Command("ping")]
        [Alias("latency")]
        [Summary("Shows the websocket connection's latency and time it takes for me send a message.")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();

            var msg = await ReplyAsync($"**Response**: ...");

            sw.Stop();

            await msg.ModifyAsync(x => x.Content = $"**Response**: {sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
