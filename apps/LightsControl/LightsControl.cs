using System.Reactive.Concurrency;

namespace HomeAutomationsNetDaemon.apps.LightsControl
{
    [NetDaemonApp]
    public class LightsControl
    {
        private readonly ILogger<LightsControl> _logger;

        public LightsControl(IHaContext ha, IScheduler scheduler, ILogger<LightsControl> logger)
        {
            _logger = logger;
            // netdaemon2 code to clean up when I buy new light sensor 
            //ha.Entity("sensor.aqara_light_out_illuminance_lux").StateChanges()
            //    .Where(e => double.Parse(e.New?.State) <= 20.0)
            //    .NDSameStateFor(TimeSpan.FromMinutes(2))
            //    .Where(e => DateTime.Now.Hour > 16 && DateTime.Now.Hour < 24)
            //    .Subscribe(s =>
            //    {
            //        TurnOnOutsideLamps();
            //    });

            // scheduler.RunEvery(TimeSpan.FromHours(24), SwitchOffTime(), () => TurnOffOutsideLamps(ha));
            scheduler.ScheduleCron("0 0 * * *", () => TurnOffOutsideLamps(ha)); 
            scheduler.ScheduleCron("0 2 * * *", () => TurnOffGateLamp(ha));

            // SunEntity sun = new Entities(ha).Sun.Sun;
            // sun.StateChanges()
            //     .Where(e => e.Old?.State == "above_horizon" && e.New?.State == "below_horizon") 
            //     .Subscribe(_ => TurnOnOutsideLamps(ha));
            

            NumericSensorEntity lightSensor = new Entities(ha).Sensor.LightSensorIlluminanceLux;
            _logger.LogInformation("{LightSensorState}", lightSensor.State);

            lightSensor.StateChanges()
                .WhenStateIsFor(e => e?.State <= 20.0, TimeSpan.FromSeconds(30))
                .Where(s => DateTime.Now.Hour is > 16 and < 24)
                .Subscribe(_ => _logger.LogInformation("works! {Test}", lightSensor.State));
            
            lightSensor.StateAllChanges()
                .Where(e => e.New?.State < 20 && e.Old?.State > e.New?.State)
                .Subscribe(_ => TurnOnOutsideLamps(ha));
        }

        private DateTimeOffset SwitchOffTime()
        {
            DateTimeOffset time = new DateTimeOffset(DateTime.Now.Date.AddDays(1) + TimeSpan.FromMinutes(1));

            _logger.LogInformation("New start time for outside lights off: {Time}", time);
            return time;
        }

        private void TurnOnOutsideLamps(IHaContext ha)
        {
            Entities entities = new Entities(ha);
            entities.Switch.SwitchTerrace.TurnOn();
            entities.Switch.SwitchEntranceDriveway.TurnOn();
            entities.Switch.SwitchGate.TurnOn();
            _logger.LogInformation("outside and gate lamps turned on");
        }

        private void TurnOffOutsideLamps(IHaContext ha)
        {
            Entities entities = new Entities(ha);
            entities.Switch.SwitchTerrace.TurnOff();
            entities.Switch.SwitchEntranceDriveway.TurnOff();
            _logger.LogInformation("outside lamps turned off");
        }

        private void TurnOffGateLamp(IHaContext ha)
        {
            SwitchEntity gateSwitch = new Entities(ha).Switch.SwitchGate;
            gateSwitch.TurnOff();
            _logger.LogInformation("gate lamps turned off");
        }
    }
}