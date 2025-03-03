using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VoltflowAPI.Contexts;

public class IdentityContext : IdentityDbContext<Account>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }
}
