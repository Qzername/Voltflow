namespace VoltflowAPI.Services;

/// <summary>
/// Creates jwt tokens from account object
/// </summary>
public interface IAccountTokenGenerator
{
    public string GenerateJwtToken(Account user, bool isAdmin);
    /// <summary>
    /// Generates a temporary token for two factor authentication that contains email information but does not allow access to other endpoints
    /// </summary>
    public string GenerateTwoFactorToken(Account user);
}
