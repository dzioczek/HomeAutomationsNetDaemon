using HomeAssistantGenerated;
using HomeAutomationsNetDaemon.apps.MorningAlarm;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using Serilog;

namespace AutomationsTesting;

public class UnitTest1
{
    [Fact]
    public void TestAlarmClock()
    {
        var haContextMoq = new Mock<IHaContext>();
        var log = new Mock<ILogger>(); 
        var testScheduler = new Microsoft.Reactive.Testing.TestScheduler();
        testScheduler.AdvanceTo(new DateTime(2023, 10, 27, 5, 54, 0).ToUniversalTime().Ticks);

        var app = new MorningAlarm(haContextMoq.Object, testScheduler); 
        
        haContextMoq.VerifyNoOtherCalls();
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        
        haContextMoq.Verify(h => h.CallService("light", "turn_on",
            It.Is<ServiceTarget>(s => s.EntityIds!.Single() == "light.bulb_master_bed"),
            It.IsAny<LightTurnOnParameters>()));

    }
} 