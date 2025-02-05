namespace VoltflowAPI.Services;

/// <summary>
/// Creates jwt tokens from account object
/// </summary>
public interface IAccountTokenGenerator
{
    public string GenerateJwtToken(Account user);
}
