using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        [Priority(1)]
        [Summary("Give information about the user")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync()
        {
            var userInfo = Context.Message.Author;       
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithImageUrl(userInfo.GetAvatarUrl());
            await ReplyAsync($"Username: {userInfo.Username}#{userInfo.Discriminator}");
            await ReplyAsync($"User Id : {userInfo.Id}");
            await ReplyAsync($"Avatar : ");
            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Priority(2)]
        [Summary("Give information about the mentionned user")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(SocketGuildUser user)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithImageUrl(user.GetAvatarUrl());
            await ReplyAsync($"Username: {user.Username}#{user.Discriminator}");
            await ReplyAsync($"User Id : {user.Id}");
            await ReplyAsync($"Avatar : ");
            await ReplyAsync("", false, embedBuilder.Build());
        }


        [Command("jitsi")]
        [Summary("link to Jitsi")]
        [Alias("appel", "videocall")]
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




