using System;
using System.Data;

using ScriptsLibV2.Databases;

namespace VoiceTime.Database
{
	internal class VoicePerChannel : DatabaseObject
	{
		public static VoicePerChannel Load(ulong userId)
		{
			DataTable selection = Db.SQLiteDatabase.Select("VoicePerChannel", "ID", $"UserId = {userId}");

			if (selection.Rows.Count > 0)
			{
				long id = Convert.ToInt32(selection.Rows[0]["ID"]);

				VoicePerChannel vpc = new VoicePerChannel();
				vpc.LoadFromDatabase(id);
				return vpc;
			}
			return null;
		}

		private ulong UserId { get; set; }
		private ulong ChannelId { get; set; }
		private ulong ServerId { get; set; }
		private long TimeInVoice { get; set; }
		private long Date { get; set; }

		public VoicePerChannel(ulong userId, ulong channelId, ulong serverId, long timeInVoice) : base(Db.SQLiteDatabase, "VoicePerChannel")
		{
			SetUserId(userId);
			SetChannelId(channelId);
			SetServerId(serverId);
			SetTimeInVoice(timeInVoice);
			SetDate(DateTimeOffset.UtcNow.Ticks);
		}

		public VoicePerChannel() : this(0UL, 0UL, 0UL, 0L) { }

		[Getter("UserID")]
		public ulong GetUserId()
		{
			return UserId;
		}

		[Setter("UserID", typeof(ulong))]
		public void SetUserId(ulong userId)
		{
			UserId = userId;
		}

		[Getter("ChannelID")]
		public ulong GetChannelId()
		{
			return ChannelId;
		}

		[Setter("ChannelID", typeof(ulong))]
		public void SetChannelId(ulong channelId)
		{
			ChannelId = channelId;
		}

		[Getter("ServerID")]
		public ulong GetServerId()
		{
			return ServerId;
		}

		[Setter("ServerID", typeof(ulong))]
		public void SetServerId(ulong serverId)
		{
			ServerId = serverId;
		}

		[Getter("TimeInVoice")]
		public long GetTimeInVoice()
		{
			return TimeInVoice;
		}

		[Setter("TimeInVoice", typeof(long))]
		public void SetTimeInVoice(long timeInVoice)
		{
			TimeInVoice = timeInVoice;
		}

		[Getter("Date")]
		public long GetDate()
		{
			return Date;
		}

		[Setter("Date", typeof(long))]
		public void SetDate(long date)
		{
			Date = date;
		}
	}
}
