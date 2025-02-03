using Microsoft.EntityFrameworkCore;

namespace VoltflowAPI.Contexts;

public class AccountsContext(DbContextOptions<AccountsContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }
}
