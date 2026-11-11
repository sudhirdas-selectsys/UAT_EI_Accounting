using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Entities.Internal.Enums;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Accounting.Utilities.Internal
{
    public class CommonUtility
    {
        static IPolicyDetailsRepository _policyDetailsRepository = new PolicyDetailsRepository();
        static IInvoiceRepository _invoiceRepository = new InvoiceRepository();
        static IBaseRepository _baseRepository = new BaseRepository();
        static IClientsRepository _accountRepository = new ClientsRepository();
        static IUsersRepository _usersRepository = new UsersRepository();
        static IEntitySettingsReposiroty _entitySettingsReposiroty = new EntitySettingsRepository();
        public IHttpContextAccessor _httpContextAccessor { get; set; }
        public static T DeepClone<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
        public static void BindLookupData(object obj, List<LookupEntity> lookUpEntities)
        {
            if (lookUpEntities == null)
                return;
            PropertyInfo[] propertyInfos = obj.GetType().GetProperties();
            foreach (var options in lookUpEntities)
            {
                foreach (var property in propertyInfos)
                {
                    if (property.Name.ToLower().Trim() == options.Name.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("%", "").Replace("(", "").Replace(")", "").Replace("/", "").ToLower())
                    {
                        property.SetValue(obj, Convert.ChangeType(options.Value, property.PropertyType));
                    }

                }
            }
        }
        public static string ConvertToCurrencyFormat(long? input, bool IgnoreCurrencySymbol = false)
        {
            var value = string.Format(new CultureInfo("en-US", false), "{0:C0}", input);
            if (IgnoreCurrencySymbol)
            {
                value = value.Substring(1).Trim();
            }
            return value;
        }
        public static string ConvertToCurrencyFormat(double? input, bool IgnoreCurrencySymbol = false, int? decimalPoints = null)
        {
            if (input == null) return "";
            var value = string.Format(new CultureInfo("en-US", false), "{0:C" + (decimalPoints ?? 0) + "}", input);
            if (IgnoreCurrencySymbol)
            {
                value = value.Substring(1).Trim();
            }
            return value;
        }
        public static string ApplyTemplateFromObject(string template, object obj)
        {
            if (string.IsNullOrEmpty(template) || obj == null)
                return template;

            var properties = obj.GetType().GetProperties();
            string result = template;

            foreach (var prop in properties)
            {
                string placeholder = "{{" + prop.Name + "}}";
                string value = prop.GetValue(obj)?.ToString() ?? "";

                result = result.Replace(placeholder, value);
            }

            return result;
        }
        public static T? GetEntitySettingValue<T>(long clientId, byte? entityTypeId, long? entityId, string settingcode)
        {
            EntitySettings settings = new EntitySettings()
            {
                EntityType_Id = entityTypeId ?? 0,
                Client_Id = clientId,
                Entity_Id = entityId ?? 0,
                SettingCode = settingcode
            };
            var setting = _entitySettingsReposiroty.GetEntitySettings(settings)?.FirstOrDefault(x => x.SettingCode == settingcode);

            if (setting == null)
                return default;

            // Text-based values
            if (typeof(T) == typeof(string))
                return (T)(object)(setting.SettingValue_Text ?? string.Empty);

            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(setting.SettingValue_Text!);

            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(setting.SettingValue_Text!);

            // Binary values
            if (typeof(T) == typeof(byte[]))
                return (T)(object)(setting.SettingValue_Binary ?? Array.Empty<byte>());

            throw new InvalidOperationException(
                $"Unsupported setting type {typeof(T).Name}");
        }
        public static double CalculateApplyAmountInPartialPayment(InvoiceDetails lineitem, List<InvoiceDetails> lineItems, int lineItemTypeId, List<InvoiceDetails> AllAvailableInvoiceLineItems, double AllocateAmountPerItem)
        {
            double _ApplyAmount = 0;
            bool isHouseCommissionType = lineItemTypeId == MasterLookup._LineItemTypes.Commission && lineItems.Any(li =>
            {
                var BillingType_LineItems_Directions = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == li.BillingType_LineItems_Directions_Id);
                var LineItems_Directions = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingType_LineItems_Directions?.LineItems_Directions_Id);
                var LineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItems_Directions?.LineItems_Clients_Id);
                var LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(z => z.LineItem_Id == LineItems_Clients?.LineItem_Id);
                return LineItems?.Name == MasterLookup._CustomLineitems.HouseCommission;
            });
            if (isHouseCommissionType)
            {
                double ActualPremium = 0;
                double Premium = 0;
                double HseCommissionAmount = 0;
                foreach (var invoiceDetails in AllAvailableInvoiceLineItems)
                {
                    if (invoiceDetails.LOB_Id == lineitem.LOB_Id)
                    {
                        LU_BillingType_LineItems_Directions _BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_BillingTypes _BillingTypes = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.BillingType_Id == _BillingLineItemsDirections?.BillingType_Id);
                        LU_LineItems_Directions _LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == _BillingLineItemsDirections?.LineItems_Directions_Id);
                        LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == _LineItemsDirections?.LineItems_Clients_Id);
                        LU_LineItems _LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id)!;
                        if (_BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Agency)
                        {
                            if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                            {
                                Premium = invoiceDetails.Amount;
                            }
                            else if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                            {
                                HseCommissionAmount = invoiceDetails.Amount;
                            }
                        }
                    }
                }
                ActualPremium = Premium + HseCommissionAmount;
                double HseCommPercentage = ActualPremium != 0 ? (HseCommissionAmount / ActualPremium) * 100 : 0;
                double calculatedHCAmount = Math.Round(((double)AllocateAmountPerItem * HseCommPercentage / 100), 2);
                _ApplyAmount = Math.Round(Math.Sign(lineitem.RemainingBalance) * Math.Min(Math.Abs(calculatedHCAmount), Math.Abs(lineitem.RemainingBalance)), 2);

            }
            else
            {
                _ApplyAmount = Math.Round(Math.Sign(lineitem.RemainingBalance) * Math.Min(Math.Abs(AllocateAmountPerItem), Math.Abs(lineitem.RemainingBalance)), 2);
            }
            return _ApplyAmount;
        }
        public static double GetEntityCommissionAmount(List<CommissionRules> commissionRules, string TransactionType, double entityCommissionBaseAmount, bool split = false)
        {
            double EntityCommissionAmount = 0;
            var EntityCommissionRule = commissionRules?.FirstOrDefault(x => (x.ApplyOnAllTransactions == true) || string.Equals(x.SubmissionType, TransactionType, StringComparison.OrdinalIgnoreCase));
            if (EntityCommissionRule != null)
            {
                if (EntityCommissionRule?.CommissionType_Id == MasterLookup._CommissionTypes.PERCENT)
                {
                    EntityCommissionAmount = Math.Round((entityCommissionBaseAmount * Convert.ToDouble(EntityCommissionRule?.Value)) / 100, 2);
                }
                else
                {
                    EntityCommissionAmount = Convert.ToDouble(EntityCommissionRule?.Value);
                }

                if (split)
                {
                    EntityCommissionAmount = Math.Round(EntityCommissionAmount / 2, 2);
                }
            }
            return EntityCommissionAmount;
        }
        public static string CleanHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string cleaned = input
                .Replace("\\\"", "\"")
                .Replace("\\/", "/");

            cleaned = Regex.Replace(cleaned, "<br\\s*>", "<br />", RegexOptions.IgnoreCase);

            cleaned = WebUtility.HtmlDecode(cleaned);

            cleaned = Regex.Replace(cleaned, "class=\".*?\"", "");
            cleaned = Regex.Replace(cleaned, "style=\".*?\"", "");

            return cleaned;
        }
        public static long GetEntityIdByName(byte EntityType_Id, string Name, long Client_Id)
        {
            try
            {
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
                    else if (EntityType_Id == MasterLookup._EntityTypes.State)
                        namesList = _policyDetailsRepository.GetAllStateDetailsByName(Name, Client_Id);
                    else if (EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                        namesList = _policyDetailsRepository.GetAllFinanceDetailsByName(Name, Client_Id);
                    else if (EntityType_Id == MasterLookup._EntityTypes.Others)
                        namesList = _policyDetailsRepository.GetAllOtherEnityDetailsByName(Name, Client_Id);
                    else if (EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                    {
                        var wholesalers = _policyDetailsRepository.GetAllWholesalersByName(Name, Client_Id);
                        var carriers = _policyDetailsRepository.GetAllCarriersByName(Name, Client_Id);
                        namesList = (wholesalers ?? new List<LookupEntity>())
                                    .Concat(carriers ?? new List<LookupEntity>())
                                    .ToList();
                    }
                    else if (EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                        namesList = _policyDetailsRepository.GetAllCarriersByName(Name, Client_Id);
                    else if (EntityType_Id == MasterLookup._EntityTypes.SalesRep)
                        namesList = _policyDetailsRepository.GetAllSalesRepsByName(Name, Client_Id);
                    else if (EntityType_Id == MasterLookup._EntityTypes.Underwriter)
                        namesList = _policyDetailsRepository.GetAllUnderwritersByName(Name, Client_Id);
                }

                return namesList?.FirstOrDefault(x => x.Name == Name)?.Value is long id ? id : 0;
            }
            catch
            {
                return 0;
            }
        }

        public static List<InvoiceDetails> GetInvoiceDetailsListWithDescription(List<InvoiceDetails> InvoiceDetailsList, PolicyDetails policyDetails, bool Is_Invoice = false)
        {
            //double ActualPremium = 0;
            //double ARPremium = 0;
            //double AgtCommissionAmount = 0;
            //double AgtCommissionPercent = 0;
            //double HseCommissionAmount = 0;
            //double TotalCommissionAmount = 0;
            //double CarrierPayable = 0;
            //double FeeAmount = 0;

            //double FEPARPremium = 0;
            //double FEPAgtCommissionAmount = 0;
            //double FEPAgtCommissionPercent = 0;
            //double FEPHseCommissionAmount = 0;
            //double FEPCarrierPayable = 0;
            //double FEPTotalCommissionAmount = 0;
            //double? OriginalAgtComissionPercent = 0;
            //OriginalAgtComissionPercent = policyDetails.PolicyDetailsLineItemsList.FirstOrDefault()?.AgentCommission;
            var GroupedByCoverage = InvoiceDetailsList?.GroupBy(o => o.LOB_Id);
            bool useLobNameForInvoiceDescription = Convert.ToBoolean(LookupUtility.GetClientSettings(policyDetails.Client_Id, MasterLookup._ClientSettings.UseLobNameForInvoiceDescription));
            if (GroupedByCoverage?.Count() > 0)
            {
                foreach (var group in GroupedByCoverage)
                {
                    double ActualPremium = 0;
                    double ARPremium = 0;
                    double AgtCommissionAmount = 0;
                    double AgtCommissionPercent = 0;
                    double HseCommissionAmount = 0;
                    double TotalCommissionAmount = 0;
                    double CarrierPayable = 0;
                    double FeeAmount = 0;

                    double FEPARPremium = 0;
                    double FEPAgtCommissionAmount = 0;
                    double FEPAgtCommissionPercent = 0;
                    double FEPHseCommissionAmount = 0;
                    double FEPCarrierPayable = 0;
                    double FEPTotalCommissionAmount = 0;
                    var InvoiceDetailsListByLob = InvoiceDetailsList?.Where(o => o.LOB_Id == group.Key);
                    if (InvoiceDetailsListByLob?.Count() > 0)
                    {
                        foreach (var invoiceDetails in InvoiceDetailsListByLob)
                        {
                            LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()?.FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                            LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions()?.FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);
                            LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients()?.FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);
                            LU_LineItems LineItems = LookupUtility.GetAllLineItems()?.FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                            LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes()?.FirstOrDefault(c => c.LineItemType_Id == LineItems?.LineItemType_Id);
                            LU_LOBs LOB = LookupUtility.GetAllLOBs()?.FirstOrDefault(x => x.LOB_Id == invoiceDetails?.LOB_Id)!;
                            invoiceDetails.Is_Commissionable = lineItems_Clients?.Is_Commissionable ?? false;
                            invoiceDetails.TreatAsPremium = lineItems_Clients?.TreatAsPremium ?? false;
                            invoiceDetails.LineItemType_Id = LineType?.LineItemType_Id ?? 0;                            
                            
                            if (lineItems_Clients?.TreatAsPremium == false)
                            {
                                if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Agency || BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                                {

                                    if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        if (useLobNameForInvoiceDescription)
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} - {LineItems.Name}";
                                        }
                                        else if (lineItems_Clients?.Is_Passthrough == true)
                                        {
                                            invoiceDetails.Description = $"{LOB.Code} - {LineItems.Name}";
                                        }
                                        else
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} {LineItems.Name}";
                                        }
                                        invoiceDetails.LineTypeName = $"{policyDetails.TransactionType} {LineType?.Name}";
                                        ARPremium = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        CarrierPayable = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        if (LineItems.LineItem_Id == MasterLookup._LineItems.AgencyBrokerCommission)
                                        {
                                            AgtCommissionAmount += Math.Abs(invoiceDetails.Amount) * Math.Sign(ARPremium);
                                        }
                                        else
                                        {
                                            HseCommissionAmount = invoiceDetails.Amount;
                                        }
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN)
                                    {
                                        HseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && (LineItemsDirections?.Direction_Id == MasterLookup._Directions.C || LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR))
                                    {
                                        AgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                }
                                else if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Direct)
                                {
                                    if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        AgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItem_Id == MasterLookup._LineItemTypes.Commission && !(LineItems.Name == MasterLookup._CustomLineitems.TotalCommissionPercentage) && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN)
                                    {
                                        HseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        invoiceDetails.Sequence = 1;
                                    }
                                }
                                else if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Insured)
                                {
                                    if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {                                        
                                        if (useLobNameForInvoiceDescription)
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} - {LineItems.Name}";
                                        }
                                        else if (lineItems_Clients?.Is_Passthrough == true)
                                        {
                                            invoiceDetails.Description = $"{LOB.Code} - {LineItems.Name}";
                                        }
                                        else
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} {LineItems.Name}";
                                        }
                                        invoiceDetails.LineTypeName = $"{policyDetails.TransactionType} {LineType?.Name}";
                                        ARPremium = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        CarrierPayable = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        TotalCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN)
                                    {
                                        HseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        AgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                }
                            }
                            else if(lineItems_Clients?.TreatAsPremium == true)
                            {
                                if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Agency || BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                                {
                                     if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR )
                                    {                                       
                                        if (useLobNameForInvoiceDescription)
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} - {LineItems.Name}";
                                        }
                                        else if (lineItems_Clients?.Is_Passthrough == true)
                                        {
                                            invoiceDetails.Description = $"{LOB.Code} - {LineItems.Name}";
                                        }
                                        else
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} {LineItems.Name}";
                                        }
                                        invoiceDetails.LineTypeName = $"{policyDetails.TransactionType} {LineType?.Name}";
                                        FEPARPremium = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP )
                                    {
                                        FEPCarrierPayable = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR )
                                    {
                                        if (LineItems.LineItem_Id == MasterLookup._LineItems.AgencyBrokerCommission)
                                        {
                                            FEPAgtCommissionAmount += Math.Abs(invoiceDetails.Amount) * Math.Sign(FEPARPremium);
                                        }
                                        else
                                        {
                                            FEPHseCommissionAmount = invoiceDetails.Amount;
                                        }
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN )
                                    {
                                        invoiceDetails.Description = $"{LineItems.Name}(FEP)";
                                        FEPHseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && (LineItemsDirections?.Direction_Id == MasterLookup._Directions.C || LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR))
                                    {
                                        FEPAgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                }
                                else if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Direct)
                                {
                                    if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        FEPAgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItem_Id == MasterLookup._LineItemTypes.Commission && !(LineItems.Name == MasterLookup._CustomLineitems.TotalCommissionPercentage) && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN)
                                    {
                                        FEPHseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        invoiceDetails.Sequence = 1;
                                    }
                                }
                                else if (BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Insured)
                                {
                                    if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {                                       
                                        if (useLobNameForInvoiceDescription)
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} - {LineItems.Name}";
                                        }
                                        else if (lineItems_Clients?.Is_Passthrough == true)
                                        {
                                            invoiceDetails.Description = $"{LOB.Code} - {LineItems.Name}";
                                        }
                                        else
                                        {
                                            invoiceDetails.Description = $"{LOB.Name} {LineItems.Name}";
                                        }
                                        invoiceDetails.LineTypeName = $"{policyDetails.TransactionType} {LineType?.Name}";
                                        FEPARPremium = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        FEPCarrierPayable = invoiceDetails.Amount;
                                        invoiceDetails.Sequence = 1;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                    {
                                        FEPTotalCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.IN)
                                    {
                                        FEPHseCommissionAmount = invoiceDetails.Amount;
                                    }
                                    else if (LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                                    {
                                        FEPAgtCommissionAmount = invoiceDetails.Amount;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(invoiceDetails.Description))
                            {                                
                                if (useLobNameForInvoiceDescription)
                                {
                                    invoiceDetails.Description = $"{LOB.Name} - {LineItems.Name}";
                                }
                                else
                                {
                                    invoiceDetails.Description = $"{LOB.Code} - {LineItems?.Name}";
                                }
                                //invoiceDetails.Description = $"{LineItems.Name} - {LOB.Code}";
                            }
                            if (string.IsNullOrEmpty(invoiceDetails.LineTypeName))
                            {
                                invoiceDetails.LineTypeName = LineType?.Name;
                            }
                        }

                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails?.BillType)?.BillingType_Id ?? 0;

                        var PremiumInvoiceDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) &&
                        ((x.Description.Contains("Premium", StringComparison.OrdinalIgnoreCase) &&
                        (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)) ||
                        (x.Description.Contains("Total Commission", StringComparison.OrdinalIgnoreCase) &&
                        BillingType_Id == MasterLookup._BillingTypes.Direct)));

                        var FEPPremiumInvoiceDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) &&
                        ((x.Description.Contains(MasterLookup._CustomLineitems.FullyEarnedPremium, StringComparison.OrdinalIgnoreCase) &&
                        (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed))
                        //||
                        //(x.Description.Contains("Total Commission", StringComparison.OrdinalIgnoreCase) &&
                        //BillingType_Id == MasterLookup._BillingTypes.Direct)
                        ));

                        var HouseCommissionInvoiceDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && (x.Description.Contains("House Commission", StringComparison.OrdinalIgnoreCase)) && x.TreatAsPremium == false);
                        var TotalCommissionDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && (x.Description.Contains("Total Commission", StringComparison.OrdinalIgnoreCase)) && BillingType_Id == MasterLookup._BillingTypes.Insured && x.TreatAsPremium == false);

                        var FEPHouseCommissionInvoiceDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && (x.Description.Contains("House Commission", StringComparison.OrdinalIgnoreCase)) && x.TreatAsPremium == false);
                        var FEPTotalCommissionDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && (x.Description.Contains("Total Commission", StringComparison.OrdinalIgnoreCase)) && BillingType_Id == MasterLookup._BillingTypes.Insured && x.TreatAsPremium == false);

                        var AgencyComissionInvoiceDetails = InvoiceDetailsListByLob.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && ((x.Description.Contains("Agency/Broker Commission", StringComparison.OrdinalIgnoreCase))));
                        if (PremiumInvoiceDetails != null)
                        {
                            if (Is_Invoice)
                            {
                                if (HouseCommissionInvoiceDetails != null)
                                {
                                    PremiumInvoiceDetails.RemainingBalance += HouseCommissionInvoiceDetails.RemainingBalance;
                                }
                                if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                {
                                    PremiumInvoiceDetails.RemainingBalance += (TotalCommissionDetails?.RemainingBalance ?? 0);
                                }
                            }
                            if (PremiumInvoiceDetails.Description.Contains("Carrier Premium") && Is_Invoice)
                            {
                                PremiumInvoiceDetails.Description = PremiumInvoiceDetails.Description.Replace("Carrier ", "");
                            }
                            PremiumInvoiceDetails.AgentCommission = AgtCommissionAmount;
                            AgtCommissionAmount = BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed ? Math.Abs(AgtCommissionAmount) : AgtCommissionAmount;
                            if (BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed && CarrierPayable < 0)
                            {
                                AgtCommissionAmount = -AgtCommissionAmount;
                            }
                            var totalPremium = CarrierPayable + AgtCommissionAmount + HseCommissionAmount;
                            PremiumInvoiceDetails.ActualPremium = totalPremium;
                            PremiumInvoiceDetails.RemainingBalance = totalPremium - AgtCommissionAmount;
                            PremiumInvoiceDetails.AgentCommissionPercent =
                                totalPremium != 0
                                ? Math.Round((AgtCommissionAmount / totalPremium) * 100, 2)
                                : 0;
                            PremiumInvoiceDetails.AgentCommissionPercent = PremiumInvoiceDetails.AgentCommissionPercent == 0 ? 0 : PremiumInvoiceDetails.AgentCommissionPercent;
                            PremiumInvoiceDetails.HouseCommission = HseCommissionAmount;
                            if (BillingType_Id == MasterLookup._BillingTypes.Direct && AgencyComissionInvoiceDetails != null)
                            {
                                PremiumInvoiceDetails.AgentCommission = AgencyComissionInvoiceDetails.Amount;
                                //PremiumInvoiceDetails.AgentCommissionPercent = AgencyComissionInvoiceDetails.Amount;
                            }
                        }
                        if (FEPPremiumInvoiceDetails != null)
                        {
                            if (Is_Invoice)
                            {
                                if (FEPHouseCommissionInvoiceDetails != null)
                                {
                                    FEPPremiumInvoiceDetails.RemainingBalance += FEPHouseCommissionInvoiceDetails.RemainingBalance;
                                }
                                if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                {
                                    FEPPremiumInvoiceDetails.RemainingBalance += (FEPTotalCommissionDetails?.RemainingBalance ?? 0);
                                }
                            }

                            if (FEPPremiumInvoiceDetails.Description.Contains("Carrier Premium") && Is_Invoice)
                            {
                                FEPPremiumInvoiceDetails.Description = FEPPremiumInvoiceDetails.Description.Replace("Carrier ", "");
                            }
                            FEPPremiumInvoiceDetails.AgentCommission = FEPAgtCommissionAmount;
                            var totalPremium = FEPCarrierPayable + FEPAgtCommissionAmount + FEPHseCommissionAmount;
                            FEPPremiumInvoiceDetails.ActualPremium = totalPremium;
                            FEPPremiumInvoiceDetails.RemainingBalance = totalPremium - FEPAgtCommissionAmount;
                            FEPPremiumInvoiceDetails.AgentCommissionPercent =
                                totalPremium != 0
                                ? Math.Round((FEPAgtCommissionAmount / totalPremium) * 100, 2)
                                : 0;

                            FEPPremiumInvoiceDetails.HouseCommission = FEPHseCommissionAmount;
                        }


                        var CommissionableLineItems = InvoiceDetailsListByLob.Where(x => x.Is_Commissionable && x.TreatAsPremium == false).ToList();
                        if (CommissionableLineItems?.Count() > 0)
                        {
                            foreach (var commissionableLineItem in CommissionableLineItems)
                            {
                                var billingDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()?.FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == commissionableLineItem.BillingType_LineItems_Directions_Id);
                                var lineItemDir = LookupUtility.GetAllLineItems_Directions()?.FirstOrDefault(x => x.LineItems_Directions_Id == billingDirections?.LineItems_Directions_Id);

                                // Check if current item is NOT 'C'
                                if (lineItemDir?.Direction_Id != MasterLookup._Directions.C)
                                {
                                    // 2. Resolve the LineItem_Id for the current item to match against others
                                    var currentLineItemClients = LookupUtility.GetAllLineItems_Clients()?.FirstOrDefault(x => x.LineItems_Clients_Id == lineItemDir?.LineItems_Clients_Id);
                                    var currentLineItemId = currentLineItemClients?.LineItem_Id;

                                    // 3. Find the "C" counterpart in the list
                                    var matchingTaxItem = CommissionableLineItems.FirstOrDefault(other =>
                                    {
                                        // Trace the 'other' item back to its Direction and LineItem_Id
                                        var oBilling = LookupUtility.GetAllLU_BillingType_LineItems_Directions()?.FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == other.BillingType_LineItems_Directions_Id);
                                        var oDir = LookupUtility.GetAllLineItems_Directions()?.FirstOrDefault(x => x.LineItems_Directions_Id == oBilling?.LineItems_Directions_Id);
                                        var oClient = LookupUtility.GetAllLineItems_Clients()?.FirstOrDefault(x => x.LineItems_Clients_Id == oDir?.LineItems_Clients_Id);

                                        return oClient?.LineItem_Id == currentLineItemId && oDir?.Direction_Id == MasterLookup._Directions.C;
                                    });

                                    // 4. Perform calculations
                                    commissionableLineItem.AgentCommission = matchingTaxItem?.Amount ?? 0;
                                    commissionableLineItem.ActualPremium = commissionableLineItem.Amount + commissionableLineItem.AgentCommission;
                                    commissionableLineItem.AgentCommissionPercent = (commissionableLineItem.ActualPremium > 0) ? Math.Round((commissionableLineItem.AgentCommission / commissionableLineItem.ActualPremium) * 100, 2) : 0;
                                }
                            }
                        }

                    }
                }
            }

            return InvoiceDetailsList?.OrderByDescending(z => z.Sequence)?.ToList();
        }
        public string GetPredefinedParameterValue(int predefinedParameter)
        {
            if (predefinedParameter == MasterLookup._LUPredefinedParameters.IMSUserID)
                return GetCurrentUserId().ToString();
            else if (predefinedParameter == MasterLookup._LUPredefinedParameters.IMSUserID)
                return GetCurrentUserGUID().ToString();
            return null;
        }
        public Guid? GetCurrentUserGUID()
        {
            var claims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity)?.Claims;
            if (claims == null || claims.Count() == 0)
                return null;
            var UserGUID = claims?.Where(c => c.Type == "UserGUID").First().Value;
            if (!string.IsNullOrEmpty(UserGUID))
            {
                return new Guid(UserGUID);
            }
            return null;
        }
        public int? GetCurrentUserId()
        {
            var strUserId = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity)?
                                .Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            int? UserId = null;
            if (!string.IsNullOrEmpty(strUserId))
            {
                UserId = Convert.ToInt32(strUserId);
            }
            return UserId;
        }

        // Excel Dropdowns Code
        public static void Adddropdown(IXLWorksheet hiddenSheet, IXLWorksheet mainSheet, List<string> values, string rangeName, string headerName)
        {
            var headerCell = mainSheet.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(headerName, StringComparison.OrdinalIgnoreCase));
            if (headerCell == null)
            {
                throw new InvalidOperationException($"{headerName} not found. Please contact the administrator or try again later.");
            }

            int colIndex = headerCell.Address.ColumnNumber;
            int hiddenColIndex = hiddenSheet.LastColumnUsed()?.ColumnNumber() + 1 ?? 1;

            var existing = mainSheet.Workbook.NamedRanges.FirstOrDefault(n => n.Name == rangeName);
            if (existing != null)
            {
                mainSheet.Workbook.NamedRanges.Delete(rangeName);
            }

            for (int i = 0; i < values.Count; i++)
            {
                hiddenSheet.Cell(i + 1, hiddenColIndex).Value = values[i];
            }

            var namedRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(values.Count, hiddenColIndex));
            mainSheet.Workbook.NamedRanges.Add(rangeName, namedRange);


            int lastRow = Math.Max(mainSheet.LastRowUsed().RowNumber(), 100);
            var targetRange = mainSheet.Range(2, colIndex, lastRow, colIndex);

            var dv = targetRange.SetDataValidation();
            dv.List($"={rangeName}");
            dv.IgnoreBlanks = true;
            dv.InCellDropdown = true;
        }
        public static void AdddropdownEntityTypeNames(XLWorkbook workbook, IXLWorksheet hiddenSheet, IXLWorksheet ws, long Client_Id)
        {
            if (workbook.NamedRanges.Count() > 0)
            {
                foreach (var name in workbook.NamedRanges.ToList())
                {
                    workbook.NamedRanges.Delete(name.Name);
                }
            }
            var entityTypes = LookupUtility.GetAllEntityTypes().ToList();
            var createdEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var entityTypeMap = new List<(string entityType, string Name)>();
            int hiddenColIndex = hiddenSheet.LastColumnUsed()?.ColumnNumber() + 1 ?? 1;

            var entityTypeNames = entityTypes.Select(et => et.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
            for (int i = 0; i < entityTypeNames.Count; i++)
            {
                hiddenSheet.Cell(i + 1, hiddenColIndex).Value = entityTypeNames[i];
            }
            var entityTypeRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(entityTypeNames.Count, hiddenColIndex));
            workbook.NamedRanges.Add("EntityTypeList", entityTypeRange);
            int entityTypeColIndex = hiddenColIndex;
            hiddenColIndex++;

            var entityTypeHeader = ws.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType, StringComparison.OrdinalIgnoreCase));
            if (entityTypeHeader != null)
            {
                int lastRow = Math.Max(ws.LastRowUsed()?.RowNumber() ?? 100, 100);
                var targetRange = ws.Range(2, entityTypeHeader.Address.ColumnNumber, lastRow, entityTypeHeader.Address.ColumnNumber);
                var dv = targetRange.SetDataValidation();
                dv.List("=EntityTypeList");
                dv.IgnoreBlanks = true;
                dv.InCellDropdown = true;
            }

            if (entityTypes.Count > 0)
            {
                foreach (var et in entityTypes)
                {
                    var values = new List<string>();
                    if (et.EntityType_Id == MasterLookup._EntityTypes.Agency)
                        values = _policyDetailsRepository.GetAllAgentDetailsByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                        values = _policyDetailsRepository.GetAllCarriersByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.Insured)
                        values = _policyDetailsRepository.GetAllInsuredsByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.Client)
                        values = _policyDetailsRepository.GetAllClientsByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                        values = _policyDetailsRepository.GetAllWholesalersByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                        values = _policyDetailsRepository.GetAllFinanceDetailsByName("", Client_Id)?.Select(x => x.Name).ToList();
                    else if (et.EntityType_Id == MasterLookup._EntityTypes.State)
                        values = _policyDetailsRepository.GetAllStateDetailsByName("", Client_Id)?.Select(x => x.Name).ToList();

                    string baseentityType = SanitizeRangeName(et.Name);
                    string entityTypeName = EnsureUniqueName(workbook, baseentityType, createdEntityNames);
                    entityTypeMap.Add((et.Name, entityTypeName));

                    List<string> cleaned = values?.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToList() ?? new List<string>();
                    if (!cleaned.Any())
                    {
                        hiddenSheet.Cell(1, hiddenColIndex).Value = string.Empty;
                        var blankRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(1, hiddenColIndex));
                        workbook.NamedRanges.Add(entityTypeName, blankRange);
                        hiddenColIndex++;
                        continue;
                    }

                    for (int i = 0; i < cleaned.Count; i++)
                    {
                        hiddenSheet.Cell(i + 1, hiddenColIndex).Value = cleaned[i];
                    }

                    var entityNamedRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(cleaned.Count, hiddenColIndex));
                    workbook.NamedRanges.Add(entityTypeName, entityNamedRange);
                    hiddenColIndex++;
                }

                int mapStartCol = hiddenSheet.LastColumnUsed()?.ColumnNumber() + 1 ?? hiddenColIndex;
                hiddenSheet.Cell(1, mapStartCol).Value = string.Empty;
                hiddenSheet.Cell(1, mapStartCol + 1).Value = string.Empty;

                var distinctMap = entityTypeMap.GroupBy(x => x.entityType).Select(g => g.First()).ToList();
                for (int i = 0; i < distinctMap.Count; i++)
                {
                    hiddenSheet.Cell(i + 2, mapStartCol).Value = distinctMap[i].entityType;
                    hiddenSheet.Cell(i + 2, mapStartCol + 1).Value = distinctMap[i].Name;
                }

                int mapRows = distinctMap.Count;
                var mapFirstCell = hiddenSheet.Cell(2, mapStartCol);
                var mapLastCell = hiddenSheet.Cell(1 + mapRows, mapStartCol + 1);
                string sheetQuoted = $"'{hiddenSheet.Name}'";
                string mapRangeAddress = $"{sheetQuoted}!${mapFirstCell.Address.ColumnLetter}${mapFirstCell.Address.RowNumber}:${mapLastCell.Address.ColumnLetter}${mapLastCell.Address.RowNumber}";

                var entityTypeHeaderCell = ws.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType, StringComparison.OrdinalIgnoreCase));
                var entityNameHeaderCell = ws.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName, StringComparison.OrdinalIgnoreCase));
                if (entityTypeHeaderCell == null || entityNameHeaderCell == null)
                    throw new InvalidOperationException("Required headers (Entity Type / Entity Name) not found in the template.");

                string parentColLetter = entityTypeHeaderCell.Address.ColumnLetter;
                int nameColNumber = entityNameHeaderCell.Address.ColumnNumber;
                int lastRow = Math.Max(ws.LastRowUsed()?.RowNumber() ?? 100, 100);
                var targetRange = ws.Range(2, nameColNumber, lastRow, nameColNumber);
                //string dvFormula = $"=INDIRECT(VLOOKUP({parentColLetter}2,{mapRangeAddress},2,FALSE))";
                string dvFormula = $"=IF({parentColLetter}2=\"\",\"\",INDIRECT(VLOOKUP({parentColLetter}2,{mapRangeAddress},2,FALSE)))";
                var dv = targetRange.SetDataValidation();
                dv.List(dvFormula);
                dv.IgnoreBlanks = true;
                dv.InCellDropdown = true;
            }
        }

        //public static void AdddropdownEntityTypeNames(XLWorkbook workbook, IXLWorksheet hiddenSheet, IXLWorksheet ws, long Client_Id)
        //{
        //    var entityTypes = LookupUtility.GetAllEntityTypes().ToList();
        //    var createdEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //    var parentMap = new List<(string Parent, string Name)>();
        //    int hiddenColIndex = hiddenSheet.LastColumnUsed()?.ColumnNumber() + 1 ?? 1;
        //    if (entityTypes.Count > 0)
        //    {
        //        foreach (var et in entityTypes)
        //        {
        //            var values = new List<string>();
        //            if (et.EntityType_Id == MasterLookup._EntityTypes.Agency)
        //            {
        //                values = _policyDetailsRepository.GetAllAgentDetailsByName("", Client_Id)?.Select(x => x.Name).ToList();
        //            }
        //            else if (et.EntityType_Id == MasterLookup._EntityTypes.Carrier)
        //            {
        //                values = _policyDetailsRepository.GetAllCarriersByName("", Client_Id)?.Select(x => x.Name).ToList();
        //            }
        //            else if (et.EntityType_Id == MasterLookup._EntityTypes.Insured)
        //            {
        //                values = _policyDetailsRepository.GetAllInsuredsByName("", Client_Id)?.Select(x => x.Name).ToList();
        //            }
        //            else if (et.EntityType_Id == MasterLookup._EntityTypes.Client)
        //            {
        //                values = _policyDetailsRepository.GetAllClientsByName("", Client_Id)?.Select(x => x.Name).ToList();
        //            }
        //            else if (et.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
        //            {
        //                values = _policyDetailsRepository.GetAllWholesalersByName("", Client_Id)?.Select(x => x.Name).ToList();
        //            }

        //            string baseParentType = SanitizeRangeName(et.Name);
        //            string parentName = EnsureUniqueName(workbook, baseParentType, createdEntityNames);
        //            parentMap.Add((et.Name, parentName));

        //            if (values != null && values.Count > 0)
        //            {
        //                var cleaned = values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToList();
        //                if (!cleaned.Any())
        //                {
        //                    hiddenSheet.Cell(1, hiddenColIndex).Value = string.Empty;
        //                    var blankRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(1, hiddenColIndex));
        //                    var existingIfAny = workbook.NamedRanges.FirstOrDefault(n => n.Name.Equals(parentName, StringComparison.OrdinalIgnoreCase));
        //                    if (existingIfAny != null) workbook.NamedRanges.Delete(existingIfAny.Name);
        //                    workbook.NamedRanges.Add(parentName, blankRange);
        //                    hiddenColIndex++;
        //                    continue;
        //                }
        //                for (int i = 0; i < cleaned.Count; i++)
        //                {
        //                    hiddenSheet.Cell(i + 1, hiddenColIndex).Value = cleaned[i];
        //                }
        //                var namedRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(cleaned.Count, hiddenColIndex));
        //                var existing = workbook.NamedRanges.FirstOrDefault(n => n.Name.Equals(parentName, StringComparison.OrdinalIgnoreCase));
        //                if (existing != null) workbook.NamedRanges.Delete(existing.Name);

        //                workbook.NamedRanges.Add(parentName, namedRange);
        //                hiddenColIndex++;
        //            }
        //            else
        //            {
        //                hiddenSheet.Cell(1, hiddenColIndex).Value = string.Empty;
        //                var blankRange = hiddenSheet.Range(hiddenSheet.Cell(1, hiddenColIndex), hiddenSheet.Cell(1, hiddenColIndex));
        //                var existing = workbook.NamedRanges.FirstOrDefault(n => n.Name.Equals(parentName, StringComparison.OrdinalIgnoreCase));
        //                if (existing != null) workbook.NamedRanges.Delete(existing.Name);
        //                workbook.NamedRanges.Add(parentName, blankRange);
        //                hiddenColIndex++;
        //            }
        //        }

        //        int mapStartCol = hiddenSheet.LastColumnUsed()?.ColumnNumber() + 1 ?? hiddenColIndex;
        //        hiddenSheet.Cell(1, mapStartCol).Value = string.Empty;
        //        hiddenSheet.Cell(1, mapStartCol + 1).Value = string.Empty;

        //        var distinctMap = parentMap.GroupBy(x => x.Parent).Select(g => g.First()).ToList();
        //        for (int i = 0; i < distinctMap.Count; i++)
        //        {
        //            hiddenSheet.Cell(i + 2, mapStartCol).Value = distinctMap[i].Parent;
        //            hiddenSheet.Cell(i + 2, mapStartCol + 1).Value = distinctMap[i].Name;
        //        }

        //        int mapRows = distinctMap.Count;
        //        var mapFirstCell = hiddenSheet.Cell(2, mapStartCol);
        //        var mapLastCell = hiddenSheet.Cell(1 + mapRows, mapStartCol + 1);

        //        string sheetQuoted = $"'{hiddenSheet.Name}'";
        //        string mapRangeAddress = $"{sheetQuoted}!${mapFirstCell.Address.ColumnLetter}${mapFirstCell.Address.RowNumber}:${mapLastCell.Address.ColumnLetter}${mapLastCell.Address.RowNumber}";

        //        var entityTypeHeaderCell = ws.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType, StringComparison.OrdinalIgnoreCase));
        //        var entityNameHeaderCell = ws.Row(1).Cells().FirstOrDefault(c => c.GetString().Trim().Equals(MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName, StringComparison.OrdinalIgnoreCase));
        //        if (entityTypeHeaderCell == null || entityNameHeaderCell == null)
        //        {
        //            throw new InvalidOperationException("Required headers (Entity Type / Entity Name) not found in the template.");
        //        }

        //        string parentColLetter = entityTypeHeaderCell.Address.ColumnLetter;
        //        int nameColNumber = entityNameHeaderCell.Address.ColumnNumber;
        //        int lastRow = Math.Max(ws.LastRowUsed()?.RowNumber() ?? 100, 100);
        //        var targetRange = ws.Range(2, nameColNumber, lastRow, nameColNumber);
        //        string dvFormula = $"=INDIRECT(VLOOKUP({parentColLetter}2,{mapRangeAddress},2,FALSE))";
        //        var dv = targetRange.SetDataValidation();
        //        dv.List(dvFormula);
        //        dv.IgnoreBlanks = true;
        //        dv.InCellDropdown = true;
        //    }
        //}

        public static List<string> GetAllPaymentUploadColumns()
        {
            var columnMappings = new Enum_PaymentsUploadColumnPropertyMappings();
            var columnNames = typeof(Enum_PaymentsUploadColumnPropertyMappings).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(string)).Select(p => p.GetValue(columnMappings)?.ToString()).Where(v => !string.IsNullOrEmpty(v)).ToList();
            return columnNames;
        }
        public static Payments MapPaymentsUploadColumnToPayments(Dictionary<string, string> rowData, out List<UploadErrorMessage> errorMessageList, long Client_Id)
        {
            var namesList = new List<LookupEntity>();
            errorMessageList = new List<UploadErrorMessage>();
            Payments payments = new Payments();
            if (rowData != null)
            {
                payments.Client_Id = Client_Id;
                payments.SourceType_Id = MasterLookup._Sources.Payment;
                foreach (var row in rowData)
                {
                    string errorMessage = string.Empty;
                    var value = row.Value;
                    string ColumnName = row.Key.Trim().ToString().ToLower();
                    if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.EntityType_Id = LookupUtility.GetAllEntityTypes().FirstOrDefault(x => string.Equals(x.Name, value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase))?.EntityType_Id ?? 0;
                            if (payments.EntityType_Id <= 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.EntityType, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDirection.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.PaymentDirection_Id = LookupUtility.GetAllPaymentDirections().FirstOrDefault(x => string.Equals(x.Name, value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase))?.PaymentDirection_Id ?? 0;
                            if (payments.PaymentDirection_Id == 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDirection, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDirection, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            if (payments.EntityType_Id > 0)
                            {
                                if (payments.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                    namesList = _policyDetailsRepository.GetAllAgentDetailsByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                    namesList = _policyDetailsRepository.GetAllCarriersByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                    namesList = _policyDetailsRepository.GetAllInsuredsByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.Client)
                                    namesList = _policyDetailsRepository.GetAllClientsByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                    namesList = _policyDetailsRepository.GetAllWholesalersByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                    namesList = _policyDetailsRepository.GetAllFinanceDetailsByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                                else if (payments.EntityType_Id == MasterLookup._EntityTypes.State)
                                    namesList = _policyDetailsRepository.GetAllStateDetailsByName(value.Trim(), Client_Id) ?? new List<LookupEntity>();
                            }

                            if (namesList.Count > 0)
                            {
                                payments.Entity_Id = Convert.ToInt64(namesList.FirstOrDefault(x => x.Name.Trim() == value.Trim().ToString())?.Value ?? 0);
                                if (payments.Entity_Id == 0)
                                {
                                    errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                                }
                            }
                            else
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.EntityName, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDate.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            var parts = value.ToString().Split(' ')[0].Split('/', '-');
                            if (parts.Length == 3 && parts[2].Length == 4)
                            {
                                DateTime _paymentDate;
                                string dateOnly = Convert.ToDateTime(value).ToString("MM-dd-yyyy");
                                bool isValidDate = DateTime.TryParseExact(dateOnly, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _paymentDate);
                                if (isValidDate && _paymentDate != DateTime.MinValue && _paymentDate.Date <= DateTime.Today)
                                {
                                    payments.PaymentDate = _paymentDate;
                                }
                                else
                                {
                                    errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDate, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                                }
                            }
                            else
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDate, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDate, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.Amount.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.Amount = (double.TryParse(value?.ToString()?.Trim(), out double parsedAmount) && parsedAmount > 0) ? parsedAmount : 0;
                            payments.RemainingAmount = payments.Amount;
                            if (payments.Amount == 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.Amount, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.Amount, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }

                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentType.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.PaymentType_Id = LookupUtility.GetAllPaymentTypes().FirstOrDefault(x => string.Equals(x.Name, value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase))?.PaymentType_Id ?? 0;
                            if (payments.PaymentType_Id == 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentType, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentType, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.ReferenceNo.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.ReferenceNumber = value.ToString().Trim();
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.ReferenceNo, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentStatus.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.PaymentStatus_Id = LookupUtility.GetAllPaymentStatus().FirstOrDefault(x => string.Equals(x.Name, value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase))?.PaymentStatus_Id ?? 0;
                            if (payments.PaymentStatus_Id == 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentStatus, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentStatus, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                    else if (ColumnName == MasterLookup._PaymentsUploadColumnPropertyMappings.BankAccount.ToLower())
                    {
                        if (IsNotNullOrEmpty(value))
                        {
                            payments.OperatingBank_Id = LookupUtility.GetAllOperatingBanks().FirstOrDefault(x => x.Client_Id == Client_Id && string.Equals(x.Name, value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase))?.OperatingBank_Id ?? 0;
                            if (payments.OperatingBank_Id == 0)
                            {
                                errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.BankAccount, Message = MasterLookup._PaymentsUploadErrorMessages.InvalidData });
                            }
                        }
                        else
                        {
                            errorMessageList.Add(new UploadErrorMessage() { PropertyName = MasterLookup._PaymentsUploadColumnPropertyMappings.BankAccount, Message = MasterLookup._PaymentsUploadErrorMessages.Required });
                        }
                    }
                }
            }
            return payments;
        }
        public static bool IsNotNullOrEmpty(object value)
        {
            return (value == null || value == DBNull.Value || value?.ToString().Trim() == "" ? false : true);
        }
        public static string SanitizeRangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Range";

            string cleaned = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (char.IsDigit(cleaned.FirstOrDefault()))
                cleaned = "_" + cleaned;

            if (string.IsNullOrEmpty(cleaned))
                cleaned = "Range";

            return cleaned;
        }
        public static string EnsureUniqueName(XLWorkbook wb, string entityTypeName, HashSet<string> entityNames)
        {
            string name = entityTypeName;
            int i = 1;
            while (wb.NamedRanges.Any(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) || entityNames.Contains(name))
            {
                name = $"{entityTypeName}_{i++}";
            }
            entityNames.Add(name);
            return name;
        }
        public static string ConvertPlanNameToCode(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            name = name.Trim();
            name = Regex.Replace(name, @"[^A-Za-z0-9\- ]", "");

            //var match = Regex.Match(name, @"^(\d+)\s*month$", RegexOptions.IgnoreCase);
            //if (match.Success)
            //{
            //    return $"M{match.Groups[1].Value}";
            //}

            //if (name.Contains("annual", StringComparison.OrdinalIgnoreCase))
            //{
            //    return "ANNUAL";
            //}
            var words = name.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            string letters = "";
            string numbers = "";

            foreach (var word in words)
            {
                if (int.TryParse(word, out _))
                {
                    numbers += word;
                }
                else
                {
                    letters += char.ToUpper(word[0]);
                }
            }

            return letters + numbers;
        }


        public static string GetMimeType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".csv" => "text/csv",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
        public static string ConvertToPhoneFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var digits = Regex.Replace(input, "[^0-9]", "");
            if (digits.Length != 10) return input;

            return Convert.ToInt64(digits).ToString("(###) ###-####");
        }
        public static string NormalizeDocNumber(string docNumber, int? lastIndex)
        {
            var match = Regex.Match(docNumber, @"^(.*?)(?:\((\d+)\))?$");

            if (!match.Success)
                return docNumber;

            string baseNumber = match.Groups[1].Value.Trim();

            int nextIndex = (lastIndex ?? 0) + 1;

            return $"{baseNumber} ({nextIndex})";
        }
        public static string MaskEmailFixed(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email;

            // Get domain
            string domain = email.Contains("@")
                ? email.Substring(email.IndexOf('@'))
                : "@xxxx.com";

            // Get first 2 characters safely
            string firstTwo;

            if (email.Length >= 2)
                firstTwo = email.Substring(0, 2);
            else if (email.Length == 1)
                firstTwo = email.Substring(0, 1);
            else
                return "xx" + domain;

            return $"{firstTwo}xx{domain}";
        }

    }
}
