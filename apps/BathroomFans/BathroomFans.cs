using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;


namespace HomeAutomationsNetDaemon.apps.BathroomFans
{
    [NetDaemonApp]
    public class BathroomFans : IAsyncInitializable
    {
        record ServiceData(string? Action, int Value);
        private readonly ILogger _logger;
        private readonly IHaContext _haContext;
        private readonly INetDaemonScheduler _scheduler;
        private readonly IHomeAssistantApiManager _apiManager;
        private CancellationToken _cancellationToken;

        public BathroomFans(ILogger<BathroomFans> logger, IHaContext ha, INetDaemonScheduler scheduler, IHomeAssistantApiManager apiManager)
        {
            _logger = logger;
            _haContext = ha;
            _scheduler = scheduler;
            _apiManager = apiManager;
    
            ha.RegisterServiceCallBack<ServiceData>("upstairs_fan_service", FanService);

            NumericSensorEntity hum = new Entities(_haContext).Sensor.AqaraTempBathroomUpstairsHumidity;

            hum.StateChanges()
               .Where(e => e.New?.State >= 60.0 && e.Old?.State < e.New?.State)
               .Subscribe(_ => CheckChangeAndRunFan());
        }
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return;
        }

        private async void FanService(ServiceData data)
        {
            SwitchEntity fan = new Entities(_haContext).Switch.TradfriPlugAttic;

            _logger.LogInformation("upstairs_fan_service called action: {Action} value: {Value}", data.Action, data.Value);
            if (data.Action == "on")
            {
                if (fan.State == "off")
                {
                    fan.TurnOn();
                    _scheduler.RunIn(TimeSpan.FromMinutes(data.Value), () => fan.TurnOff());
                }
            }

            if (data.Action == "off")
            {
                if (fan.State == "on") fan.TurnOff();
            }
        }

        private async void CheckChangeAndRunFan()
        {
            if (await GetChangeDynamicPercet() > 3.0) FanService(new ServiceData("on", 20));
        } 

        private async Task<double> GetChangeDynamicPercet()
        {
            List<HassState> states = await _apiManager.GetHumidityHistory(_cancellationToken);

            int numberOfLastChanges = 5; 

            var arr = states.OrderBy(s => s.LastUpdated).TakeLast(numberOfLastChanges).ToArray();

            double indexesMultiplication = 1; // first chain index for that is always 1 as we don't have previos value

            for (int i = 1; i < arr.Length; i++) // skipping first as this was set above
            {
                indexesMultiplication *= (Convert.ToDouble(arr[i].State, CultureInfo.InvariantCulture) / Convert.ToDouble(arr[i - 1].State, CultureInfo.InvariantCulture));
            }
                        
            double percentChange = (Math.Pow(indexesMultiplication, (1 / ((double)numberOfLastChanges - 1))) - 1) * 100; 
            
            return percentChange;
        }


    }
}
