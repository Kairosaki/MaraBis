using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MaraBis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaraBis.Modules
{
    public class InteractionCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }

        private CommandHandler _commandHandler;

        public InteractionCommands (CommandHandler handler)
        {
            _commandHandler = handler;
        }

        [SlashCommand("jitsi", "jitsi DevOps 2022")]
        public async Task Jitsi()
        {
            ComponentBuilder componentBuilder = new ComponentBuilder()
                .WithButton("Vers Jitsi", style: ButtonStyle.Link, url: "https://meet.jit.si/DevOps21Technifutur_fai9Ee");
            await RespondAsync("Cliquez sur le bouton pour vous rediriger vers la vidéoconférence Jitsi :", components: componentBuilder.Build());
        }

        [SlashCommand("whois", "user information(username, id, avatar)")]
        [Alias("user", "userinfo")]
        public async Task WhoIs(IUser pUser = null)
        {
            var user = pUser ?? Context.User;

            var embedInfos = new EmbedBuilder()
                .AddField("Username", user.Username, true)
                .AddField("User ID", user.Id)
                .AddField("Compte crée le :", user.CreatedAt)
                .WithColor(Color.Purple)
                .WithImageUrl(CDN.GetUserAvatarUrl(user.Id, user.AvatarId, 512, ImageFormat.Auto))
                .Build();
            await RespondAsync($"{user.Mention} Voici tes informations", embed: embedInfos);
        }
        

    }
}
