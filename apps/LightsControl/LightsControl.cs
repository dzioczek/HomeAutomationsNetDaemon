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
            

            NumericSensorEntity lightSensor = new Entities(ha).Sensor.LightSensorOutsideIlluminanceLux;
            _logger.LogInformation("{LightSensorState}", lightSensor.State);
            
            lightSensor.StateChanges()
                .WhenStateIsFor(e => e?.State <= 20.0, TimeSpan.FromMinutes(5), scheduler)
                .Subscribe(_ => TurnOnOutsideLamps(ha));
            
            //lightSensor.StateAllChanges()
            //    .Where(e => e.New?.State < 20 && e.Old?.State > e.New?.State)
            //    .Subscribe(_ => TurnOnOutsideLamps(ha));
        }

        private void TurnOnOutsideLamps(IHaContext ha)
        {
            Entities entities = new Entities(ha);
            entities.Light.TerraceLamps.TurnOn();
            entities.Light.DrivewayLamps.TurnOn();
            entities.Light.GateLamps.TurnOn();
            entities.Switch.PlugEnder.TurnOn();
            _logger.LogInformation("outside and gate lamps turned on");
        }

        private void TurnOffOutsideLamps(IHaContext ha)
        {
            Entities entities = new Entities(ha);
            entities.Light.TerraceLamps.TurnOff();
            entities.Light.DrivewayLamps.TurnOff();
            entities.Switch.PlugEnder.TurnOff();
            _logger.LogInformation("outside lamps turned off");
        }

        private void TurnOffGateLamp(IHaContext ha)
        {
            LightEntity gateSwitch = new Entities(ha).Light.GateLamps;
            gateSwitch.TurnOff();
            _logger.LogInformation("gate lamps turned off");
        }
    }
}