using Discord;
using Discord.Addons.Hosting.Util;
using Discord.Addons.Hosting;
using Discord.WebSocket;

namespace Marabis.Services
{
    public class BotStatusService : DiscordClientService
    {
        public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger) : base(client, logger)
        {

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitForReadyAsync(stoppingToken);
            Logger.LogInformation("Client is Ready !!");

            await Client.SetActivityAsync(new Game("Mara-thon !"));
        }
    }
}
