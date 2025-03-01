using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Contexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    public DbSet<Car> Cars { get; set; }
    public DbSet<ChargingStation> ChargingStations { get; set; }
    public DbSet<ChargingPort> ChargingPorts { get; set; }
    public DbSet<ChargingStationOpeningHours> ChargingStationOpeningHours { get; set; }
    public DbSet<ChargingStationsServiceHistory> ChargingStationServiceHistory { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}
