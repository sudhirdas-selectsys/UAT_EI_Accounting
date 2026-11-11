using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.EI;
using Accounting.Utilities.Internal;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text;
using Accounting.Entities.Internal;
using System.ComponentModel.Design;
using Newtonsoft.Json.Linq;
using Accounting.Entities.Internal.Enums;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.DBUtilities.Implementations.Transactions;
using Newtonsoft.Json;

namespace Accounting.Utilities.External.EI_Connect
{
    public class EI_ConnectUtility
    {
        ServiceLogInfo _serviceLogInfo = new ServiceLogInfo() { Service_Id = MasterLookup._Services.EIConnect };
        ServiceResponse serviceResponse = new ServiceResponse();
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        Enum_APIServiceDetails enum_APIServiceDetails = new Enum_APIServiceDetails();

        public ServiceResponse CommissionDocumentsUpload(long ClientID, List<DocumentFileInfo> documentFileInfos)
        {
            
            var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(ClientID).Where(x => x.Service_Id == MasterLookup._Services.EIConnect);
            if (ClientLevelParameters?.Count() > 0)
            {
                enum_APIServiceDetails.Service_Url = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.Value;
                enum_APIServiceDetails.APIKey = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.Value;
            }
            List<EIUploadDocumentRequest> LstEIUploadDocuments = new List<EIUploadDocumentRequest>();

            if (documentFileInfos == null || documentFileInfos.Count == 0)
            {
                return serviceResponse;
            }
            else if (documentFileInfos?.Count > 0)
            {
                foreach (var item in documentFileInfos)
                {
                    int lastIndex = item.Name.LastIndexOf('_');
                    if (lastIndex > -1)
                    {
                        item.Name = item.Name.Substring(0, lastIndex) + ".pdf";
                    }
                    LstEIUploadDocuments.Add(new EIUploadDocumentRequest
                    {
                        BrokerID = item.Agent_Id,
                        TypeID = MasterLookup._EIBrokerStatementDocType,
                        TypeDescription = "Broker Statement",
                        FileName = item.Name,
                        FileData = item.Base64String,
                        Title = item.Name
                    });
                }

            }
            _serviceLogInfo = new ServiceLogInfo()
            {
                Service_Id = MasterLookup._Services.EIConnect,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UploadCommissionsStatements,
                Client_Id = ClientID
            };
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", "Bearer " + enum_APIServiceDetails.APIKey);

            //ServiceURL = "https://localhost:44309/api/Portals/Accounting/"; //for  testing
            HttpServiceUtility.ExecuteJsonPostRequest(_serviceLogInfo, enum_APIServiceDetails.Service_Url + "BrokerStatements", LstEIUploadDocuments, typeof(string), false, Headers, false, null);
            if (!string.IsNullOrEmpty(_serviceLogInfo.Response))
            {
                Match match = Regex.Match(_serviceLogInfo.Response, "Documents Uploaded: (\\d+) of (\\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int uploadedCount = Convert.ToInt32(match.Groups[1].Value);
                    int totalCount = Convert.ToInt32(match.Groups[2].Value);
                    if (uploadedCount == totalCount && LstEIUploadDocuments.Count == totalCount)
                    {

                        _serviceLogInfo.Is_Success = true;
                    }
                }
                if (!_serviceLogInfo.Is_Success)
                    _serviceLogInfo.Message = _serviceLogInfo.Response;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_serviceLogInfo);
            if (!string.IsNullOrEmpty(_serviceLogInfo.Response))
            {
                if (_serviceLogInfo.Is_Success)
                {
                    serviceResponse.Status = true;
                    serviceResponse.Message = _serviceLogInfo.Response;
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = _serviceLogInfo.Response;
                }
            }
            else
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Unable to Process.";
            }
            return serviceResponse;
        }

        public ServiceResponse AgentEmails(long ClientID, List<long> agentDetailIds)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            try
            {
                var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(ClientID).Where(x => x.Service_Id == MasterLookup._Services.EIConnect);
                if (ClientLevelParameters?.Count() > 0)
                {
                    enum_APIServiceDetails.Service_Url = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.Value;
                    enum_APIServiceDetails.APIKey = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.Value;
                }

                if (agentDetailIds == null || agentDetailIds.Count == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "No Agent Detail IDs provided.";
                    return serviceResponse;
                }

                _serviceLogInfo = new ServiceLogInfo()
                {
                    Service_Id = MasterLookup._Services.EIConnect,
                    ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAgentEmailData,
                    Client_Id = ClientID
                };

                Dictionary<string, string> Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", "Bearer " + enum_APIServiceDetails.APIKey);

                //enum_APIServiceDetails.Service_Url = "https://localhost:44309/api/Portals/Accounting/";

                // This is the API endpoint in EI
                var apiUrl = enum_APIServiceDetails.Service_Url + "GetAgentEmailData";

                HttpServiceUtility.ExecuteJsonPostRequest(_serviceLogInfo, apiUrl, agentDetailIds, typeof(string), false, Headers, false, null);
                var responseObject = _serviceLogInfo.Response;
                var jsonResponse = JsonConvert.SerializeObject(responseObject);

                if (!string.IsNullOrEmpty(_serviceLogInfo.Response))
                {
                    serviceResponse.Status = _serviceLogInfo.Is_Success;
                    serviceResponse.Message = _serviceLogInfo.Response;
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Unable to get agent email data.";
                }

                _ServiceLogInfoRepository.InsertServiceLogInfo(_serviceLogInfo);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}
