using HomeAssistantGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationsNetDaemon.apps.Ender
{
    [NetDaemonApp]
    public class Ender
    {
        private readonly ILogger<Ender> _logger;
        NotifyServices _services;
        public Ender(IHaContext context, IScheduler scheduler, ILogger<Ender> logger, NotifyServices services)
        {
            _logger = logger;
            _services = services;

            Entities entities = new Entities(context);

            //entities.BinarySensor.OctoprintPrinting.StateChanges()
            //    .WhenStateIsFor(e => e?.State == "off", TimeSpan.FromMinutes(15), scheduler)
            //    .Subscribe(_ => TurnOffPrinter(entities));

            
            entities.Sensor.Prusamk4.StateChanges()
                .Where(e => e.Old?.State == "printing" && e.New?.State == "idle")
                .SubscribeSafe(_ => NotifyMichal(context), _logger);
            

        }

        private void TurnOffPrinter(Entities entities)
        {
            _logger.LogInformation("Turning off printer after print.");
            entities.Switch.PlugEnder.TurnOff();
        }

        private void NotifyMichal(IHaContext context)
        {
            _services.MobileAppMichalsIphone(
                title: "Print Done!",
                message: "Your fresh print is now ready.",
                data: new { tag = "PrinterNotification" }
            );
        }
    }
}
