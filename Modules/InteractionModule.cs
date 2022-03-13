using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;

namespace Marabis.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ulong _channelId = 918606393219616798;

        [SlashCommand("jitsi", "Jitsi link DevOps 2022")]
        public async Task Jitsi()
        {
            if (_channelId != Context.Channel.Id) return;

            var builder = new ComponentBuilder().WithButton("Vers Jitsi", style: ButtonStyle.Link, url: "https://meet.jit.si/DevOps21Technifutur_fai9Ee");

            await RespondAsync("Click to join jitsi conference.", components: builder.Build());
        }

        [SlashCommand("whois", "user informations")]
        public async Task WhoIs(IUser pUser = null, ushort pSize = 0)
        {
            if (_channelId != Context.Channel.Id) return;

            var user = pUser ?? Context.User;

            ushort size = (ushort)(pSize == 0 ? 512 : pSize);

            var embed = new EmbedBuilder()
                .AddField("Username", user.Username, true)
                .AddField("User ID", user.Id)
                .AddField("Account created :", user.CreatedAt)
                .WithColor(Color.Purple)
                .WithImageUrl(CDN.GetUserAvatarUrl(user.Id, user.AvatarId, size, ImageFormat.Auto))
                .Build();
            await RespondAsync($"{user.Mention} Here is your infos", embed: embed);
        }
    }
}
