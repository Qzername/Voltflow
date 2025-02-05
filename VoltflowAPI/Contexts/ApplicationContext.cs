using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace VoltflowAPI.Contexts;

/// <summary>
/// Manages ASP.NET Identity
/// </summary>
public class ApplicationContext : IdentityDbContext<Account>
{
	public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}
