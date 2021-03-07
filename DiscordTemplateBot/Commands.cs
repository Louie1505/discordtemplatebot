using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;

namespace DiscordTemplateBot
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("sample")]
        [Summary("Placeholder command")]
        public async Task Sample()
        {
            // Access the channel from the Command Context.
            await Context.Channel.SendMessageAsync("Sample message");
        }
    }
}
