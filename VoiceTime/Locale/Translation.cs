using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using VoiceTime.Properties;

namespace VoiceTime.Locale
{
	internal class Translation
	{
		public static Translation LoadFromJson(Locale locale)
		{
			string json = Encoding.UTF8.GetString(GetLocaleResource(locale));
			Debug.WriteLine(json);
			TranslationSchema schema = JsonConvert.DeserializeObject<TranslationSchema>(json);

			return new Translation(schema);
		}

		private static byte[] GetLocaleResource(Locale locale)
		{
			switch (locale)
			{
				case Locale.ptPT:
					return Resources.pt_PT_JSON;
				case Locale.enEN:
					return Resources.en_EN_JSON;
				default: return null;
			}
		}

		private Dictionary<string, string> TranslationsDictionary = new Dictionary<string, string>();

		public Translation(TranslationSchema schema)
		{
			foreach (PropertyInfo property in schema.GetType().GetProperties())
			{
				string value = (string)property.GetValue(schema);
				TranslationsDictionary.Add(property.Name.ToLower(), value);
				Debug.WriteLine($"Add '{value}' to dictionary key '{property.Name.ToLower()}'.");
			}
		}

		public string GetTranslation(string key)
		{
			return TranslationsDictionary[key.ToLower()];
		}
	}
}
