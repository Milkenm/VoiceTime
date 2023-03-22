using Newtonsoft.Json;

namespace VoiceTime.Locale
{
	internal class TranslationSchema
	{
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("session_title")] public string SessionTitle { get; set; }
		[JsonProperty("total_title")] public string TotalTitle { get; set; }
		[JsonProperty("day")] public string Day { get; set; }
		[JsonProperty("day_plural")] public string DayPlural { get; set; }
		[JsonProperty("hour")] public string Hour { get; set; }
		[JsonProperty("hour_plural")] public string HourPlural { get; set; }
		[JsonProperty("minute")] public string Minute { get; set; }
		[JsonProperty("minute_plural")] public string MinutePlural { get; set; }
		[JsonProperty("second")] public string Second { get; set; }
		[JsonProperty("second_plural")] public string SecondPlural { get; set; }
	}
}
