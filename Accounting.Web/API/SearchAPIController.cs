using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Accounting.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Web.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SearchAPIController : BaseController
    {
        public ServiceResponse GetNames(byte EntityType_Id, string Name, long Client_Id)
        {
            var serviceResponse = new ServiceResponse();
            var namesList = new List<LookupEntity>();

            if (EntityType_Id > 0)
            {
                if (EntityType_Id == MasterLookup._EntityTypes.Agency)
                    namesList = _policyDetailsRepository.GetAllAgentDetailsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Carrier)
                    namesList = _policyDetailsRepository.GetAllCarriersByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Insured)
                    namesList = _policyDetailsRepository.GetAllInsuredsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Client)
                    namesList = _policyDetailsRepository.GetAllClientsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                    namesList = _policyDetailsRepository.GetAllWholesalersByName(Name, Client_Id);
                else if(EntityType_Id == MasterLookup._EntityTypes.State)
                     namesList = _policyDetailsRepository.GetAllStateDetailsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                    namesList = _policyDetailsRepository.GetAllFinanceDetailsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Others)
                    namesList = _policyDetailsRepository.GetAllOtherEnityDetailsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                {
                    var wholesalers = _policyDetailsRepository.GetAllWholesalersByName(Name, Client_Id);
                    var carriers = _policyDetailsRepository.GetAllCarriersByName(Name, Client_Id);
                    namesList = (wholesalers ?? new List<LookupEntity>()).Concat(carriers ?? new List<LookupEntity>()).ToList();
                }
                else if (EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                    namesList = _policyDetailsRepository.GetAllCarriersByName(Name, Client_Id);
                else if(EntityType_Id == MasterLookup._EntityTypes.SalesRep)
                    namesList = _policyDetailsRepository.GetAllSalesRepsByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Underwriter)
                    namesList = _policyDetailsRepository.GetAllUnderwritersByName(Name, Client_Id);
                else if (EntityType_Id == MasterLookup._EntityTypes.Mortgagee)
                    namesList = _policyDetailsRepository.GetAllMortgageeDetailsByName(Name, Client_Id);

            }
            else
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "No entity type was provided.";
                return serviceResponse;
            }

            serviceResponse.Data = namesList;
            serviceResponse.Status = true;
            return serviceResponse;
        }
        public ServiceResponse GetPolicyNumbers(string PolicyNumber)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (PolicyNumber == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    List<LookupEntity>? namesList = new List<LookupEntity>();
                    namesList = _transactionhistoryRepository.GetAllPolicyNumbersByNumber(PolicyNumber, userSession.Client_Id);
                    serviceResponse.Data = namesList;
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse SuggestChartofAccounts(string SearchValue)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (SearchValue == "null" || string.IsNullOrEmpty(SearchValue))
                {
                    SearchValue = null;
                }
                long EffectiveClient_Id = userSession.ConfigurationClient_Id > 0 ? userSession.ConfigurationClient_Id : userSession.Client_Id;
                Clients clients = _accountRepository.GetClients(EffectiveClient_Id);
                List<ChartofAccounts> ChartofAccount = _chartofAccountsRepository.ChartofAccountsSuggestions(clients.AccountingService_Id, EffectiveClient_Id, SearchValue);
                serviceResponse.Status = true;
                serviceResponse.Data = ChartofAccount;
                serviceResponse.Message = "Success";
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse GetInvoiceNumbers(string InvoiceNumber)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (InvoiceNumber == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    List<LookupEntity>? namesList = new List<LookupEntity>();
                    namesList = _transactionhistoryRepository.GetAllInvoiceNumbersByNumber(InvoiceNumber, userSession.Client_Id);
                    serviceResponse.Data = namesList;
                    serviceResponse.Status = true;
                }
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
