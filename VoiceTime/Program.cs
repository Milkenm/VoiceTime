using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using Discord;
using Discord.WebSocket;

using ScriptsLibV2;

using VoiceTime.Database;

namespace VoiceTime
{
	internal class Program
	{
		private static readonly DiscordBot bot = new DiscordBot(Assembly.GetEntryAssembly());
		private static readonly Dictionary<SocketUser, long> UsersInVoice = new Dictionary<SocketUser, long>();

		private static void Main(string[] args)
		{
			Console.Title = $"VoiceTime {GetVersion()}";
			Application.ApplicationExit += new EventHandler((se, ev) =>
			{
				bot.StopAsync().Wait();
			});

			bot.Client.Log += Client_Log;
			bot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

			bot.StartAsync();

			Console.ReadKey();
		}

		private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousChannel, SocketVoiceState newChannel)
		{
			// Ignore bots
			if (user.IsBot || user.IsWebhook)
			{
				return;
			}

			// Join voice
			if (previousChannel.VoiceChannel == null && newChannel.VoiceChannel != null)
			{
				OnVoiceJoin(user);
				return;
			}
			// Leave voice
			if (previousChannel.VoiceChannel != null && newChannel.VoiceChannel == null)
			{
				OnVoiceLeave(user);
				return;
			}
		}

		private static void OnVoiceJoin(SocketUser user)
		{
			UsersInVoice.Add(user, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
		}

		private static void OnVoiceLeave(SocketUser user)
		{
			long secondsInVoice = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - UsersInVoice[user];
			UsersInVoice.Remove(user);

			// Save session
			VoiceSession session = new VoiceSession(user.Id, secondsInVoice);
			session.SaveToDatabase();
			// Save total time
			Database.VoiceTime vt = Database.VoiceTime.Load(user.Id);
			if (vt == null)
			{
				vt = new Database.VoiceTime(user.Id, secondsInVoice);
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
