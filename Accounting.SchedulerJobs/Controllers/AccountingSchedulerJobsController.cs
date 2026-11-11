using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Accounting.SchedulerJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountingSchedulerJobsController : ControllerBase
    {
        private readonly ISchedulerRepositoryService schedulerRepositoryService;
        private readonly IServiceProvider serviceProvider;

        public AccountingSchedulerJobsController(ISchedulerRepositoryService schedulerRepositoryService, IServiceProvider serviceProvider)
        {
            this.schedulerRepositoryService = schedulerRepositoryService;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet("DownloadLog")]
        public IActionResult DownloadLog([Required]DateTime date, bool download = false)
        {
            try
            {
                FileLogger.LogInformation($"AccountingSchedulerJobsController: DownloadLog Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != ConfigurationHelper.SchedulerJobAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                string fileName = $"{date:ddMMMyyyy}.txt";
                string filePath = Path.Combine(ConfigurationHelper.LogsFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Log file not found.");
                }

                if (download)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", fileName);
                }

                string fileContent = System.IO.File.ReadAllText(filePath);
                return Content(fileContent, "text/plain");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"AccountingSchedulerJobsController: DownloadLog: Date: {date}: Download: {download}: Error While Processing: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetJobs")]
        public async Task<IActionResult> GetJobs()
        {
            var response = new ResponseViewModel<List<object>>();
            try
            {
                FileLogger.LogInformation($"AccountingSchedulerJobsController: GetJobs Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.Unauthorized, $"Authorization token missing.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, response);
                }

                if (token != ConfigurationHelper.SchedulerJobAuthToken)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.Unauthorized, $"Invalid token.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, response);
                }

                var jobs = await schedulerRepositoryService.GetSchedulerClientJobs();
                var listJobs = jobs.Select(x => (object)new
                {
                    x.SchedulerClientJob_Id,
                    x.Job_Id,
                    x.JobName,
                    x.CompanyName,
                    x.Client_Id,
                    x.CronExpression,
                    x.TimeZone
                }).ToList();

                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"Scheduler Jobs Fetched Successfully.", listJobs, null);
                return this.StatusCode((int)HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"AccountingSchedulerJobsController: GetJobs: Error While Processing: {ex}");
                return this.ExceptionResult(ex, response, "GetJobs");
            }
        }

        [HttpPost("RunJob")]
        public async Task<IActionResult> RunJob([FromQuery] long schedulerClientJobId, string inputField = "", string requestedBy = "Api")
        {
            var response = new ResponseViewModel<string?>();
            DateTime startTime = DateTime.Now;
            bool executionStatus = false;
            string? errorMessage = null;
            SchedulerClientJobs? jobConfig = null;
            try
            {
                FileLogger.LogInformation($"AccountingSchedulerJobsController: RunJob Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.Unauthorized, $"Authorization token missing.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, response);
                }

                if (token != ConfigurationHelper.SchedulerJobAuthToken)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.Unauthorized, $"Invalid token.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, response);
                }

                var jobs = await schedulerRepositoryService.GetSchedulerClientJobs();
                if (jobs?.Count == 0)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Scheduler Client Jobs not found.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.NotFound, response);
                }

                jobConfig = jobs?.FirstOrDefault(x => x.SchedulerClientJob_Id == schedulerClientJobId);
                if (jobConfig == null)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Job configuration not found.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.NotFound, response);
                }

                Type jobType = Type.GetType(jobConfig.JobType);

                if (jobType == null)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.BadRequest, $"Unable to load job type '{jobConfig.JobType}'.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.BadRequest, response);
                }

                var jobInstance = serviceProvider.GetService(jobType);

                if (jobInstance is not ISchedulerJob schedulerJob)
                {
                    response = Utils.ResponseMapping((int)HttpStatusCode.BadRequest, $"'{jobConfig.JobType}' does not implement ISchedulerJob.", response.Response ?? null, null);
                    return this.StatusCode((int)HttpStatusCode.BadRequest, response);
                }

                startTime = DateTime.Now;
                await schedulerJob.ExecuteAsync(jobConfig, CancellationToken.None, inputField, requestedBy);
                executionStatus = true;
                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"{jobConfig.JobName} executed successfully.", $"{jobConfig.JobName} executed successfully.", null);
                return this.StatusCode((int)HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"AccountingSchedulerJobsController: RunJob: Error While Processing: {ex}");
                errorMessage = ex.ToString();
                await Utils.SendExceptionEmail(jobConfig, requestedBy, startTime, ex);
                return this.ExceptionResult(ex, response, "RunJob");
            }
            finally
            {
                if (jobConfig != null)
                {
                    await schedulerRepositoryService.InsertExecutionLog(
                        new SchedulerJobExecutionLog
                        {
                            CreatedOrUpdatedBy = requestedBy,
                            SchedulerClientJob_Id = jobConfig.SchedulerClientJob_Id,
                            Job_Id = jobConfig.Job_Id,
                            Client_Id = jobConfig.Client_Id,
                            JobName = jobConfig.JobName,
                            ExecutionStartTime = startTime,
                            ExecutionEndTime = DateTime.Now,
                            ExecutionStatus = executionStatus,
                            ErrorMessage = errorMessage
                        }
                    );
                }
            }
        }

        private ObjectResult ExceptionResult<TType>(Exception ex, ResponseViewModel<TType> response, string methodName)
        where TType : class
        {
            response.Error.Add(new ResponseViewModel<TType>.Errors
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Description = ex.Message == null || ex.Message == string.Empty ? "Something went wrong" : ex.Message,
                Message = ex.Message,
                More_Info = ex.StackTrace,
            });
            response = Utils.ResponseMapping((int)HttpStatusCode.InternalServerError, $"Exception occurs while processing request for {methodName}.", null, response.Error);
            return this.StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}
