using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.RepositoryServices;
using System.Collections.Concurrent;

namespace Accounting.SchedulerJobs.Utilities
{
    public class ConfigurationHelper
    {
        public static List<SchedulerClientJobs> ClientSchedulerJobs { get; set; }
        public static string SchedulerJobAuthToken {  get; set; }
        public static string LogsFolder { get; set; }
        public static int LogFileRetentionDays { get; set; }
        public static string Accounting_Base_URL { get; set; }
        public static int SchedulerPollingIntervalInSeconds { get; set; }
        public static ConcurrentDictionary<long, bool> RunningJobs = new();
        public static int MaxEmailsPerMinute { get; set; }
        public static int EmailRateLimitWindowMinutes { get; set; }
        public static byte MasterLookup_EntityTypes_State { get; set; }
        public static byte MasterLookup_EntityTypes_Agency  { get; set; }
        public static byte MasterLookup_PaymentStatus_Completed { get; set; }
        public static byte MasterLookup_PaymentTypes_Bill {  get; set; }
        public static byte MasterLookup_Sources_Payment {  get; set; }
        public static byte MasterLookup_Directions_AR {  get; set; }
        public static byte MasterLookup_Directions_AP { get; set; }
        public static byte MasterLookup_PaymentDirections_Incoming { get; set; }
        public static byte MasterLookup_PaymentDirections_Outgoing { get; set; }
        public static byte MasterLookup_PaymentTypes_Check { get; set; }
        public static byte MasterLookup_PaymentStatus_Pending { get; set; }
        public static async Task Initialize(IConfiguration configuration, IWebHostEnvironment environment, ISchedulerRepositoryService schedulerRepositoryService)
        {
            SQLHelper.ConnectionString = configuration.GetConnectionString("AccountingDBConnectionString");
            SchedulerJobAuthToken = configuration.GetValue<string>("AppSettings:ExpertInsured_Accounting_UAT");
            SchedulerPollingIntervalInSeconds = Convert.ToInt32(configuration.GetValue<string>("AppSettings:SchedulerPollingIntervalInSeconds"));
            MaxEmailsPerMinute = Convert.ToInt32(configuration.GetValue<string>("AppSettings:MaxEmailsPerMinute"));
            EmailRateLimitWindowMinutes = Convert.ToInt32(configuration.GetValue<string>("AppSettings:EmailRateLimitWindowMinutes"));
            LogsFolder = Path.Combine(environment.ContentRootPath, "App_Data", "Logs");
            LogFileRetentionDays = Convert.ToInt32(configuration.GetValue<string>("AppSettings:LogFileRetentionDays"));
            Accounting_Base_URL = configuration.GetValue<string>("AppSettings:Accounting_Base_URL");
            ClientSchedulerJobs = await schedulerRepositoryService.GetSchedulerClientJobs();
            MasterLookup_EntityTypes_State = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_EntityTypes_State.ToString()));
            MasterLookup_EntityTypes_Agency = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_EntityTypes_Agency.ToString()));
            MasterLookup_PaymentStatus_Completed = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_PaymentStatus_Completed.ToString()));
            MasterLookup_PaymentTypes_Bill = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_PaymentTypes_Bill.ToString()));
            MasterLookup_Sources_Payment = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_Sources_Payment.ToString()));
            MasterLookup_Directions_AR = 1;
            MasterLookup_Directions_AP = 2;
            MasterLookup_PaymentDirections_Incoming = 1;
            MasterLookup_PaymentDirections_Outgoing = 2;
            MasterLookup_PaymentTypes_Check = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_PaymentTypes_Check.ToString()));
            MasterLookup_PaymentStatus_Pending = Convert.ToByte(await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.MasterLookup_PaymentStatus_Pending.ToString()));
        }
    }
}
