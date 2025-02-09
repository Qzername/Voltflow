using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace Voltflow.Models;

public static class JsonConverter
{
	public static StringContent ToStringContent(object? value)
	{
		string jsonString = JsonConvert.SerializeObject(value);
		return new StringContent(jsonString, Encoding.UTF8, "application/json");
	}

	public static JObject FromString(string value) => JObject.Parse(value);
}
