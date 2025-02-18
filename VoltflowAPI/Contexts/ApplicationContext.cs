using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Models.Identity;

namespace VoltflowAPI.Contexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    public DbSet<Car> Cars { get; set; }
    public DbSet<ChargingStation> ChargingStations { get; set; }
}
