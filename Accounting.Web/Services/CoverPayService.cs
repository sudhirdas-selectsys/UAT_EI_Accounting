using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.CoverPay;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Accounting.Web.Abstract;
using Microsoft.Exchange.WebServices.Data;
using System.Data;

namespace Accounting.Web.Services
{
    public class CoverPayService : ICoverPayService
    {
        private readonly string baseURL;
        private readonly List<LU_ServiceLevelParameters> serviceLevelParameters;
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        ICoverPayRepository coverPayRepository = new CoverPayRepository();
        public CoverPayService()
        {
            serviceLevelParameters = LookupUtility.GetAllServiceLevelParameters()?.Where(x => x.Service_Id == MasterLookup._Services.CorePay).ToList() ?? new List<LU_ServiceLevelParameters>();
            baseURL = serviceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.DefaultValue ?? string.Empty;
        }
        public ExternalPaymentDetails GetExternalPaymentDetails(long clientId, int serviceId, long policyDetailId)
        {
            return coverPayRepository.GetExternalPaymentDetails(clientId, serviceId, policyDetailId);
        }
        public List<CoverPayOutstandingPolicyDetails> GetOutstandingPolicyDetails(long Client_Id, long Entity_Id, byte EntityType_Id, string querySearch)
        {
            return coverPayRepository.GetOutstandingPolicyDetails(Client_Id, Entity_Id, EntityType_Id, querySearch);
        }
        public CoverPayOutstandingInvoiceDetails GetOutstandingInvoiceDetails(long clientId, string policyDetails_Id_Json)
        {
            return coverPayRepository.GetOutstandingInvoiceDetails(clientId, policyDetails_Id_Json);
        }
        public string GenerateCoverPayPaymentLink(long clientId, long policyDetail_Id, string? policyNumber, long user_Id, string? entityType, string? entityName, string? entityEmail, string? accountNumber, string? postalCode)
        {
            string paymentLink = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(entityEmail))
                {
                    entityEmail = "surendrateja.chinnari@selectsys.com";
                }

                var entitiesList = new List<entities>
                {
                    new entities
                    {
                        type = entityType,
                        value = entityName ?? string.Empty
                    },
                    new entities
                    {
                        type = "AccountNumber",
                        value = accountNumber ?? string.Empty
                    }
                };

                if (!string.IsNullOrWhiteSpace(postalCode))
                {
                    entitiesList.Add(new entities
                    {
                        type = "PostalCode",
                        value = postalCode
                    });
                }

                var coverPayGeneratePaymentLink = new CoverPayGeneratePaymentLink
                {
                    requestId = $"REQ-{Guid.NewGuid().ToString()}",
                    idempotencyKey = $"IDEMP-{Guid.NewGuid().ToString()}",
                    context = new context
                    {
                        externalSource = new externalSource
                        {
                            referenceId = $"FI-{Guid.NewGuid().ToString()}" ?? string.Empty
                        }
                    },
                    entities = entitiesList,
                    paymentLink = new paymentLink
                    {
                        purpose = "INVOICE_PAYMENT",
                        expiresAt = DateTime.Now.AddDays(1),
                        singleUse = false

                    },
                    notification = new notification
                    {
                        sendPaymentLink = true,
                        recipients = new recipients
                        {
                            to = new List<string> { entityEmail }
                        },
                    }
                };

                var token = this.GetCoverPayAuthToken(clientId);
                if (!string.IsNullOrEmpty(token))
                {
                    paymentLink = GetCoverPayPaymentLink(clientId, token, coverPayGeneratePaymentLink);
                    if (!string.IsNullOrEmpty(paymentLink))
                    {
                        var externalPaymentDetails = new ExternalPaymentDetails
                        {
                            Client_Id = clientId,
                            Service_Id = MasterLookup._Services.CorePay,
                            ExternalReferenceNo = coverPayGeneratePaymentLink?.requestId ?? string.Empty,
                            PolicyDetail_Id = policyDetail_Id,
                            PolicyNumber = policyNumber,
                            PaymentLink = paymentLink,
                        };
                        coverPayRepository.InsertUpdateExternalPaymentDetails(externalPaymentDetails, user_Id);
                    }
                }

            }
            catch (Exception)
            {
                return string.Empty;
            }
            return paymentLink;
        }

        public string GetCoverPayAuthToken(long client_Id)
        {
            var token = string.Empty;
            try
            {
                var coverPayTokenResponse = new CoverPayTokenResponse();
                var serviceLogInfo = new ServiceLogInfo()
                {
                    Service_Id = MasterLookup._Services.CorePay,
                    ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAccessToken,
                    Client_Id = client_Id
                };

                try
                {
                    string serviceURL = $"{baseURL}/api/auth/token";
                    var tokenRequest = new
                    {
                        Client_Id = serviceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Id)?.DefaultValue ?? string.Empty,
                        SecretKey = serviceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.DefaultValue ?? string.Empty,
                    };
                    coverPayTokenResponse = (CoverPayTokenResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, serviceURL, tokenRequest, typeof(CoverPayTokenResponse), false, null);
                    if (!string.IsNullOrEmpty(coverPayTokenResponse?.Data?.Token))
                    {
                        token = coverPayTokenResponse.Data?.Token;
                    }
                }
                catch (Exception ex)
                {
                    serviceLogInfo.Message = ex.Message;
                    serviceLogInfo.Is_Success = false;
                }
                finally
                {
                    _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return token;
        }

        public string GetCoverPayPaymentLink(long client_Id, string token, CoverPayGeneratePaymentLink coverPayGeneratePaymentLink)
        {
            var paymentLink = string.Empty;
            var coverPayPaymentResponse = new CoverPayPaymentResponse();
            var serviceLogInfo = new ServiceLogInfo()
            {
                Service_Id = MasterLookup._Services.CorePay,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GeneratePaymentLink,
                Client_Id = client_Id
            };

            try
            {
                string serviceURL = $"{baseURL}/api/Payment/createlinkformakepayment";
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", $"Bearer {token}");
                coverPayPaymentResponse = (CoverPayPaymentResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, serviceURL, coverPayGeneratePaymentLink, typeof(CoverPayPaymentResponse), false, headers);
                if (!string.IsNullOrEmpty(coverPayPaymentResponse?.response?.paymentLink?.paymentLink?.Url))
                {
                    paymentLink = coverPayPaymentResponse?.response?.paymentLink?.paymentLink?.Url ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                serviceLogInfo.Message = ex.Message;
                serviceLogInfo.Is_Success = false;
            }
            finally
            {
                _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
            }

            return paymentLink;
        }
    }
}
