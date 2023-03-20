using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;

namespace HomeAutomationsNetDaemon.apps.Shades;

[NetDaemonApp]
public class ShadesControl
{
    private readonly ILogger<ShadesControl> _logger;
    private readonly IHaContext _ha;

    public ShadesControl(IHaContext ha, ILogger<ShadesControl> logger, IScheduler scheduler)
    {
        _logger = logger;
        _ha = ha;

        NumericSensorEntity lightSensor = new Entities(ha).Sensor.LightSensorOutsideIlluminanceLux;
        
        lightSensor.StateChanges()
            .Where(e => e.New?.State < 20 && e.Old?.State > e.New?.State)
            .Subscribe(_ => CloseCovers());

        scheduler.ScheduleCron("0 7 * * *", () => OpenCovers());
    }

    private void CloseCovers()
    {
        Services services = new Services(_ha);
        services.Cover.CloseCover(ServiceTarget.FromEntity("cover.blind_office"));
        _logger.LogInformation("Closing covers");
    }

    private void OpenCovers()
    {
        Services services = new Services(_ha);
        services.Cover.OpenCover(ServiceTarget.FromEntity("cover.blind_office"));
        _logger.LogInformation("Opening covers");
    }
}