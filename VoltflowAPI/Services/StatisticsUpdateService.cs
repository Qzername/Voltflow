
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Statistics;

namespace VoltflowAPI.Services
{
    public class StatisticsUpdateService : BackgroundService
    {
        public static Dictionary<int,ChargingStationWeekUsage> ChargingStationWeekUsage { get; set; }
        public static Dictionary<int, int[]> ChargingStationPeekHours { get; set; }

        readonly IServiceScopeFactory _scopeFactory;
        readonly ILogger<StatisticsUpdateService> _logger;

        public StatisticsUpdateService(IServiceScopeFactory scopeFactory, ILogger<StatisticsUpdateService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            ChargingStationWeekUsage = new Dictionary<int, ChargingStationWeekUsage>();
            ChargingStationPeekHours = new Dictionary<int, int[]>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Generating statistics...");

            using (var scope = _scopeFactory.CreateScope())
            {
                var applicationContext = scope.ServiceProvider.GetService<ApplicationContext>();

                Dictionary<int, Dictionary<int, int>> statistics = new Dictionary<int, Dictionary<int,int>>();

                DateTime weekAgo = DateTime.UtcNow.AddDays(-7);
                var transcations = applicationContext.Transactions.Where(x => x.StartDate >= weekAgo);

                foreach (var t in transcations)
                {
                    if (!statistics.ContainsKey(t.ChargingStationId))
                    {
                        statistics[t.ChargingStationId] = new Dictionary<int, int>();
                        ChargingStationPeekHours[t.ChargingStationId] = new int[24];

                        for (int i = 0; i < 7; i++)
                            statistics[t.ChargingStationId][i] = 0;
                    }
                   
                    statistics[t.ChargingStationId][(int)t.StartDate.DayOfWeek]++;
                    ChargingStationPeekHours[t.ChargingStationId][t.StartDate.Hour]++;
                }    

                foreach(var kv in  statistics)
                {
                    var weekStats = kv.Value;

                    ChargingStationWeekUsage[kv.Key] = new ChargingStationWeekUsage()
                    {
                        Sunday = weekStats[0],
                        Monday = weekStats[1],
                        Tuesday = weekStats[2],
                        Wednesday = weekStats[3],
                        Thursday = weekStats[4],
                        Friday = weekStats[5],
                        Saturday = weekStats[6],
                    };
                }
            }
            
            _logger.LogInformation("Generation of statistics is done");

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
