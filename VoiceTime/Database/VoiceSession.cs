using System;
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

		public ulong UserId { get; set; }
		public long TimeInVoice { get; set; }
		public long Date { get; set; }

		public VoiceSession(ulong userId, long timeInVoice) : base(Db.SQLiteDatabase, "VoiceSessions")
		{
			SetUserId(userId);
			SetTimeInVoice(timeInVoice);
			SetDate(DateTimeOffset.UtcNow.Ticks);
		}

		public VoiceSession() : this(0, 0) { }

		[Getter("UserId")]
		public ulong GetUserId()
		{
			return UserId;
		}

		[Setter("UserId", typeof(ulong))]
		public void SetUserId(ulong userId)
		{
			UserId = userId;
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
