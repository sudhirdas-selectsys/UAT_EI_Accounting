using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.SchedulerJobs;
using Accounting.Utilities.Internal;

namespace Accounting.Utilities.External.SchedulerJobs
{
    public class SchedulerJobsUtility
    {
        public string BaseURL { get; set; }
        public string Token { get; set; }
        public long _Client_Id { get; set; }
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        public SchedulerJobsUtility(long Client_Id)
        {
            var ServiceLevelParameters = LookupUtility.GetAllServiceLevelParameters().Where(x => x.Service_Id == MasterLookup._Services.SchedulerAPI);
            BaseURL = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.DefaultValue;
            Token = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.AccessToken)?.Value;
            _Client_Id = Client_Id;
        }
        public SchedulerJobsResponse SchedulerJobsResponse(long Invoice_Id, int SchedulerClientJob_Id)
        {
            SchedulerJobsResponse response = new SchedulerJobsResponse();
            string ServiceURL = $"{BaseURL}/api/AccountingSchedulerJobs/RunJob?schedulerClientJobId={SchedulerClientJob_Id}&inputfield={Invoice_Id}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", MasterLookup.ApplicationAuthToken);
            ServiceLogInfo getLog = new ServiceLogInfo()
            {
                Service_Id = MasterLookup._Services.SchedulerAPI,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.RunJob,
                Client_Id = _Client_Id
            };
            try
            {

                response = (SchedulerJobsResponse)HttpServiceUtility.ExecuteJsonPostRequest(getLog, ServiceURL, null, typeof(SchedulerJobsResponse), false, Headers);
                _ServiceLogInfoRepository.InsertServiceLogInfo(getLog);
            }
            catch (Exception ex)
            {
                getLog.Message =ex.Message;
                getLog.Is_Success = false;
            }
            return response;
        }
    }
}
