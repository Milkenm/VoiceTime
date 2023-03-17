using ScriptsLibV2.Databases;
using ScriptsLibV2.Util;

namespace VoiceTime.Database
{
	internal class Db
	{
		public static SQLiteDB SQLiteDatabase = new SQLiteDB(string.Format(SQLiteDB.DEFAULT_CONNECTION_STRING, Utils.GetInstallationFolder() + @"\VoiceDB.db"));
	}
}
