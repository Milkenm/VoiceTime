using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using ScriptsLibV2;

using VoiceTime.Database;

namespace VoiceTime
{
	internal class Program
	{
		static DiscordBot bot = new DiscordBot(Assembly.GetEntryAssembly());
		static Dictionary<SocketUser, long> voiceActivity = new Dictionary<SocketUser, long>();

		static void Main(string[] args)
		{
			bot.Client.Log += Client_Log;
			bot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

			bot.StartAsync();

			Console.ReadKey();
		}

		private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousChannel, SocketVoiceState newChannel)
		{
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
			voiceActivity.Add(user, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			SendMessage(user.Mention + " entrou no voice.");
		}

		private static void OnVoiceLeave(SocketUser user)
		{
			long secondsInVoice = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - voiceActivity[user];

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

			SendMessage(user.Mention + $" saiu do voice ({TimeSpan.FromSeconds(secondsInVoice).ToString("HHh:MMm:SSs")}, total: {TimeSpan.FromSeconds(vt.GetTotalTime()).ToString("HHh:MMm:SSs")}).");
		}

		private static void SendMessage(string message)
		{

			var channel = bot.Client.GetChannel(742869012983054486) as IMessageChannel;
			channel.SendMessageAsync(message).GetAwaiter();
		}

		private static Task Client_Log(LogMessage arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}
	}
}
