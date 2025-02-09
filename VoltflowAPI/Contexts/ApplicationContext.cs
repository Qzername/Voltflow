using Microsoft.EntityFrameworkCore;

namespace VoltflowAPI.Contexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    public DbSet<Car> Cars { get; set; }
}
