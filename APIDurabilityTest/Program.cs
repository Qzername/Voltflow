/*
 * This test sends 1000 random station points with 1-3 charging ports
 * This requires admin account
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

const string adminEmail = "";
const string adminPassword = "";
 StringContent ToStringContent(object? value)
{
    string jsonString = JsonConvert.SerializeObject(value);
    return new StringContent(jsonString, Encoding.UTF8, "application/json");
}

HttpClient httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://voltflow-api.heapy.xyz")
};

var token = await httpClient.PostAsync("/api/Identity/Authentication/login", ToStringContent(new
{
    Email = adminEmail,
    Password = adminPassword
}));

var response = await token.Content.ReadAsStringAsync();

Console.WriteLine(response);

var jObject = JObject.Parse(response);

httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jObject["token"]!.ToString());

Random rng = new Random();

Stopwatch stopwatch = new Stopwatch();

Console.WriteLine("test ready for start");
Console.ReadLine();

stopwatch.Start();

for (int i = 0; i <1000;i++)
{
    _ = Request(); //change _ = to await to wait for each request
}

async Task Request()
{
    StringContent content = ToStringContent(new
    {
        Longitude = rng.NextDouble() * 200 - 100,
        Latitude = rng.NextDouble() * 200 - 100,
        Cost = rng.Next(0, 5),
        MaxChargeRate = rng.Next(0, 5)
    });

    var request = await httpClient.PostAsync("/api/ChargingStations", content);

    Console.WriteLine(request.StatusCode.ToString());
}

Console.ReadLine();

stopwatch.Stop();

Console.WriteLine(stopwatch.ElapsedMilliseconds);