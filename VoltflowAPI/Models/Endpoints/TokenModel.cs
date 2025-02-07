namespace VoltflowAPI.Models.Endpoints;

/// <summary>
/// Model contains token for various uses
/// Model is used in various controllers, therefore it is sepperated from the controllers structs
/// </summary>
public struct TokenModel
{
    public string Token { get; set; }
}
