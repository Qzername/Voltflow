using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace Voltflow.Models;

public static class JsonConverter
{
	public static StringContent ToStringContent(object? value)
	{
		string jsonString = Serialize(value);
		return new StringContent(jsonString, Encoding.UTF8, "application/json");
	}

	public static string Serialize(object? value) => JsonConvert.SerializeObject(value, settings: new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
	public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T?>(json);

	public static JObject FromString(string value) => JObject.Parse(value);
}
