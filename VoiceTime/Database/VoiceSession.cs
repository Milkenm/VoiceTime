﻿using System;
using System.Data;

using ScriptsLibV2.Databases;

namespace VoiceTime.Database
{
	internal class VoiceSession : DatabaseObject
	{
		public static VoiceSession Load(ulong userId)
		{
			DataTable selection = Db.SQLiteDatabase.Select("VoiceSessions", "ID", $"UserId = {userId}");

			if (selection.Rows.Count > 0)
			{
				long id = Convert.ToInt32(selection.Rows[0]["ID"]);

				VoiceSession vs = new VoiceSession();
				vs.LoadFromDatabase(id);
				return vs;
			}
			return null;
		}

		private ulong UserId { get; set; }
		private ulong ServerId { get; set; }
		private long TimeInVoice { get; set; }
		private long Date { get; set; }

		public VoiceSession(ulong userId, ulong serverId, long timeInVoice) : base(Db.SQLiteDatabase, "VoiceSessions")
		{
			SetUserId(userId);
			SetServerId(serverId);
			SetTimeInVoice(timeInVoice);
			SetDate(DateTimeOffset.UtcNow.Ticks);
		}

		public VoiceSession() : this(0UL, 0UL, 0L) { }

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
