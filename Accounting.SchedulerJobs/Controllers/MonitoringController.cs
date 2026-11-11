using Microsoft.AspNetCore.Mvc;

namespace Accounting.SchedulerJobs.Controllers
{
    /// <summary>
    /// Represents a MonitoringController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MonitoringController : ControllerBase
    {
        /// <summary>
        /// A very simple method to communicate to the monitoring system that this system is up and running.
        /// </summary>
        /// <returns>string.</returns>
        [HttpGet("monitoring")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public string MonitoringPing()
        {
            TimeZoneInfo localTz = TimeZoneInfo.Local;        
            return $"Accounting Scheduler Service is alive as of DatetimeNow: {DateTime.Now} DateTimeUtcNow : {DateTime.UtcNow} ServerTimeZoneId: [{localTz.Id}] ServerTimeZoneDisplayName: [{localTz.DisplayName}]";
        }
    }
}
