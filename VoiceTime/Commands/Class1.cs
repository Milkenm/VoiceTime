using System.Threading.Tasks;

using Discord.Commands;

namespace VoiceTime.Commands
{
	internal class Commands : ModuleBase<SocketCommandContext>
	{
		[Name("Locale"), Command("locale"), Summary("Translates the bot messages to another language.")]
		public async Task ChangeLocale(string msg)
		{
			await ReplyAsync(msg);
		}
	}
}
