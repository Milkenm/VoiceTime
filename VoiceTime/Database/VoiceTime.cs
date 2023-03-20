using System;
using System.Data;

using ScriptsLibV2.Databases;

namespace VoiceTime.Database
{
	internal class VoiceTime : DatabaseObject
	{
		public static VoiceTime Load(ulong userId)
		{
			DataTable selection = Db.SQLiteDatabase.Select("VoiceTime", "ID", $"UserId = {userId}");

			if (selection.Rows.Count > 0)
			{
				long id = Convert.ToInt32(selection.Rows[0]["ID"]);

				VoiceTime vt = new VoiceTime();
				vt.LoadFromDatabase(id);
				return vt;
			}
			return null;
		}

		private ulong UserId { get; set; }
		private long TotalTime { get; set; }

		public VoiceTime(ulong userId, long totalTime) : base(Db.SQLiteDatabase, "VoiceTime")
		{
			UserId = userId;
			TotalTime = totalTime;
		}

		public VoiceTime() : this(0, 0) { }

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

		[Getter("TotalTime")]
		public long GetTotalTime()
		{
			return TotalTime;
		}

		[Setter("TotalTime", typeof(long))]
		public void SetTotalTime(long totalTime)
		{
			TotalTime = totalTime;
		}
	}
}
