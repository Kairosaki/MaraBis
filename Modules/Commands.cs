using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaraBis.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong !");
        }

        [Command("userinfo")]
        [Summary("Give information about the user")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(SocketGuildUser user = null)
        {
            var userInfo = user ?? Context.Message.Author;
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithImageUrl(userInfo.GetAvatarUrl());
            await ReplyAsync($"Username: {userInfo.Username}#{userInfo.Discriminator}");
            await ReplyAsync($"User Id : {userInfo.Id}");
            await ReplyAsync($"Avatar : ");
            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("jitsi")]
        [Summary("link to Jitsi")]
        [Alias("jitsi", "videocall")]
        public async Task JitsiAsync()
        {
            var builder = new ComponentBuilder().WithButton("Vers jitsi", style: ButtonStyle.Link, url: "https://meet.jit.si/DevOps21Technifutur_fai9Ee");
            await ReplyAsync("Retrouvez Jitsi ici : ", components: builder.Build());
        }
    }

    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Group("clean")]
        public class CleanModule : ModuleBase<SocketCommandContext>
        {
            [Command]
            public async Task DefaultCleanAsync()
            {
                // ...
            }

            [Command("messages")]
            public async Task CleanAsync(int count)
            {
                // ...
            }
        }

        [Command("ban")]
        public Task BanAsync(IGuildUser user) =>
            Context.Guild.AddBanAsync(user);
    }
}
