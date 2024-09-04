using System.Reactive.Concurrency;

namespace HomeAutomationsNetDaemon.apps.Ender;

[NetDaemonApp]
public class Ender
{
    private readonly ILogger<Ender> _logger;
    private readonly NotifyServices _services;

    public Ender(IHaContext context, IScheduler scheduler, ILogger<Ender> logger, NotifyServices services)
    {
        _logger = logger;
        _services = services;

        Entities entities = new(context);

        //entities.BinarySensor.OctoprintPrinting.StateChanges()
        //    .WhenStateIsFor(e => e?.State == "off", TimeSpan.FromMinutes(15), scheduler)
        //    .Subscribe(_ => TurnOffPrinter(entities));


        entities.Sensor.Prusamk4.StateChanges()
            .Where(e => e.Old?.State == "printing" && e.New?.State == "idle")
            .SubscribeSafe(_ => NotifyMichal(), _logger);
    }

    private void TurnOffPrinter(Entities entities)
    {
        _logger.LogInformation("Turning off printer after print");
        entities.Switch.PlugEnder.TurnOff();
    }

    private void NotifyMichal()
    {
        _services.MobileAppGreencell(
            title: "Print Done!",
            message: "Your fresh print is now ready.",
            data: new { tag = "PrinterNotification" }
        );
    }
}