using System.Reactive.Concurrency;
using System.Threading.Tasks;
using HomeAutomationsNetDaemon.apps.Shades;
using ILogger = Serilog.ILogger;

namespace HomeAutomationsNetDaemon.apps.MorningAlarm;

[NetDaemonApp]
public class MorningAlarm
{
    private readonly ILogger<MorningAlarm> _logger;
    private readonly IHaContext _haContext;

    public MorningAlarm(IHaContext haContext, IScheduler scheduler)
    {
        _haContext = haContext;

        scheduler.ScheduleCron("55 5 * * 1-5", () => WakeUp()); 
    }


    private async void WakeUp()
    {
        LightEntity bulb = new Entities(_haContext).Light.BulbMasterBed;
        for (int i = 1; i < 256; i++)
        {
            long temp = 2202 + (i * 7); 
            bulb.TurnOn(brightness: i, kelvin: temp);
            await Task.Delay(2353);
        }
    }
}