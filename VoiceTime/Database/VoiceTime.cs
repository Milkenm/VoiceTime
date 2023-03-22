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
		private ulong ServerId { get; set; }
		private long TotalTime { get; set; }

		public VoiceTime(ulong userId, ulong serverId, long totalTime) : base(Db.SQLiteDatabase, "VoiceTime")
		{
			SetUserId(userId);
			SetServerId(serverId);
			SetTotalTime(totalTime);
		}

		public VoiceTime() : this(0UL, 0UL, 0L) { }

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
