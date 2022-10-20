using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomationsNetDaemon.apps
{
    public static class ApiExtensions
    {
        public static async Task<List<HassState>> GetHumidityHistory(this IHomeAssistantApiManager apiManager, CancellationToken cancellationToken)
        {
            var apiUrl = $"history/period?filter_entity_id=sensor.aqara_temp_bathroom_upstairs_humidity";

            var resp = await apiManager.GetApiCallAsync<List<List<HassState>>>(apiUrl, cancellationToken);
            return resp.FirstOrDefault(); 
        }
    }
}
