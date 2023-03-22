using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using Discord;
using Discord.Net;
using Discord.WebSocket;

using Newtonsoft.Json;

using ScriptsLibV2;

using VoiceTime.Database;
using VoiceTime.Locale;

namespace VoiceTime
{
	internal class Program
	{
		private static readonly DiscordBot bot = new DiscordBot(Assembly.GetEntryAssembly());
		private static readonly Dictionary<SocketUser, long> UsersInVoice = new Dictionary<SocketUser, long>();
		private static readonly Dictionary<SocketUser, long> LastVoiceMove = new Dictionary<SocketUser, long>();
		private static readonly Dictionary<Locale.Locale, Translation> Translations = new Dictionary<Locale.Locale, Translation>();

		private static void Main(string[] args)
		{
			foreach (Locale.Locale locale in Enum.GetValues(typeof(Locale.Locale)))
			{
				Translations.Add(locale, Translation.LoadFromJson(locale));
			}

			Console.Title = $"VoiceTime {GetVersion()}";
			Application.ApplicationExit += new EventHandler((se, ev) =>
			{
				bot.StopAsync().Wait();
			});

			bot.Client.Log += Client_Log;
			bot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
			bot.Client.Ready += Client_Ready;
			bot.Client.SlashCommandExecuted += SlashCommandHandler;

			bot.StartAsync();

			// Initialize all users
			foreach (SocketGuild guild in bot.Client.Guilds)
				foreach (SocketVoiceChannel voiceChannel in guild.VoiceChannels)
					foreach (SocketGuildUser connectedUser in voiceChannel.ConnectedUsers)
					{
						OnVoiceJoin(connectedUser);
					}

			Task.Delay(-1).Wait();
		}

		public static async Task Client_Ready()
		{
			SlashCommandBuilder slashLocale = new SlashCommandBuilder()
			{
				Name = "setlocale",
				Description = "Set the language of the messages.",
			};

			SlashCommandOptionBuilder localeOptions = new SlashCommandOptionBuilder()
			{
				Name = "locale",
				Description = "The language you wish to use.",
				IsRequired = true,
				Type = ApplicationCommandOptionType.String,
			};
			foreach (Locale.Locale locale in Enum.GetValues(typeof(Locale.Locale)))
			{
				string localeName = locale.ToString();
				localeOptions.AddChoice(localeName, localeName);
			}

			slashLocale.AddOption(localeOptions);

			try
			{
				await bot.Client.CreateGlobalApplicationCommandAsync(slashLocale.Build());
			}
			catch (HttpException ex)
			{
				string json = JsonConvert.SerializeObject(ex.Message, Formatting.Indented);
				Console.WriteLine(json);
			}
		}

		private static async Task SlashCommandHandler(SocketSlashCommand command)
		{
			await command.RespondAsync($"You executed {command.Data.Name}!");
		}

		private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousChannel, SocketVoiceState newChannel)
		{
			// Ignore bots
			if (user.IsBot || user.IsWebhook)
			{
				return;
			}

			// Change channels
			if (previousChannel.VoiceChannel != null)
			{
				OnVoiceChange(user, previousChannel.VoiceChannel.Id, previousChannel.VoiceChannel.Guild.Id);
			}
			// Join voice
			if (previousChannel.VoiceChannel == null && newChannel.VoiceChannel != null)
			{
				OnVoiceJoin(user);
			}
			// Leave voice
			else if (previousChannel.VoiceChannel != null && newChannel.VoiceChannel == null)
			{
				OnVoiceLeave(user, previousChannel.VoiceChannel.Guild.Id);
			}
		}

		private static void OnVoiceChange(SocketUser user, ulong channelId, ulong guildId)
		{
			long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			long secondsInChannel = unix - LastVoiceMove[user];

			LastVoiceMove.Remove(user);
			LastVoiceMove.Add(user, unix);

			VoicePerChannel vpc = new VoicePerChannel(user.Id, channelId, guildId, secondsInChannel);
			vpc.SaveToDatabase();
		}

		private static void OnVoiceJoin(SocketUser user)
		{
			UsersInVoice.Add(user, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			LastVoiceMove.Add(user, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
		}

		private static void OnVoiceLeave(SocketUser user, ulong guildId)
		{
			long secondsInVoice = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - UsersInVoice[user];
			UsersInVoice.Remove(user);
			LastVoiceMove.Remove(user);

			// Save session
			VoiceSession session = new VoiceSession(user.Id, guildId, secondsInVoice);
			session.SaveToDatabase();
			// Save total time
			Database.VoiceTime vt = Database.VoiceTime.Load(user.Id);
			if (vt == null)
			{
				vt = new Database.VoiceTime(user.Id, guildId, secondsInVoice);
			}
			else
			{
				vt.SetTotalTime(vt.GetTotalTime() + secondsInVoice);
			}
			vt.SaveToDatabase();

			// Create the embed
			EmbedBuilder embed = new EmbedBuilder
			{
				Title = "VoiceTime",
				Description = $"{user.Mention}, está aqui o tempo que já desperdiçaste nos voice channels deste maravilhoso servidor.",
				Fields =
				{
					new EmbedFieldBuilder()
					{
						Name= "Sessão",
						Value=GetTimeString(TimeSpan.FromSeconds(secondsInVoice)),
						IsInline=false
					},
					new EmbedFieldBuilder()
					{
						Name="Total",
						Value=GetTimeString(TimeSpan.FromSeconds(vt.GetTotalTime())),
						IsInline=false
					}
				},
				Footer = new EmbedFooterBuilder()
				{
					Text = $"VoiceTime {GetVersion()}",
				},
				Color = Color.Green,
				ThumbnailUrl = user.GetAvatarUrl(),
			};

			// Send to channel
			SendEmbed(embed.Build());
		}

		private static void SendEmbed(Embed embed)
		{
			foreach (ulong channelId in bot.DebugChannels)
			{
				IMessageChannel channel = bot.Client.GetChannel(channelId) as IMessageChannel;
				channel.SendMessageAsync(embed: embed).GetAwaiter();
			}
		}

		private static string GetVersion()
		{
			return $"v{Assembly.GetExecutingAssembly().GetName().Version}";
		}

		private static Task Client_Log(LogMessage arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}

		private static string GetTimeString(TimeSpan timeSpan)
		{
			return string.Format("{0:%d} dia{1}, {0:%h} hora{2}, {0:%m} minuto{3} e {0:%s} segundo{4}", timeSpan, AddPlural(timeSpan.Days), AddPlural(timeSpan.Hours), AddPlural(timeSpan.Minutes), AddPlural(timeSpan.Seconds));
		}

		private static string AddPlural(int num)
		{
			if (num != 1)
			{
				return "s";
			}
			return string.Empty;
		}
	}
}
