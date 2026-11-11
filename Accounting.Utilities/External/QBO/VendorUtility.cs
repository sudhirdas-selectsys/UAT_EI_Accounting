using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Vendor;
using Accounting.Utilities.Internal;
using Microsoft.Data.SqlClient;
namespace Accounting.Utilities.External.QBO
{
    public class VendorUtility
    {
        IPolicyDetailsRepository _PolicyDetailsRepository = new PolicyDetailsRepository();
        IClientsRepository _ClientsRepository = new ClientsRepository();
        IBaseRepository baseRepository = new BaseRepository();
        public CreateVendorReq PrepareCreateVendorReqObject(byte EntityType_Id, long Entity_Id, string EntityName=null, long Client_Id=0)
        {
            CreateVendorReq createVendorReq = new CreateVendorReq();
            if(EntityType_Id == MasterLookup._EntityTypes.Agency)
            {
                AgentDetails agentDetails = _PolicyDetailsRepository.GetAgentDetails(Entity_Id) ?? new AgentDetails();
                createVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = agentDetails.Address1,
                    Line2 = agentDetails.Address2,
                    City = agentDetails.City,
                    Country = agentDetails.Country,
                    CountrySubDivisionCode = agentDetails.State,
                    PostalCode = agentDetails.ZipCode,
                };
                //createVendorReq.GivenName = agentDetails.Name;
                //createVendorReq.FamilyName = agentDetails.Name;
                createVendorReq.CompanyName = agentDetails.Name;
                createVendorReq.TaxIdentifier = agentDetails.FEIN;
                createVendorReq.DisplayName = $"EI - {agentDetails.Name}";
                if (!string.IsNullOrWhiteSpace(agentDetails.Email))
                {
                    createVendorReq.PrimaryEmailAddr = new PrimaryEmailAddr()
                    {
                        Address = agentDetails.Email,
                    };
                }
            }
            else if(EntityType_Id == MasterLookup._EntityTypes.Carrier)
            {
                CarrierDetails carrierDetails = _PolicyDetailsRepository.GetCarrierDetails(Entity_Id) ?? new CarrierDetails();
                createVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = carrierDetails.Address1,
                    Line2 = carrierDetails.Address2,
                    City = carrierDetails.City,
                    Country = carrierDetails.Country,
                    CountrySubDivisionCode = carrierDetails.State,
                    PostalCode = carrierDetails.ZipCode,
                };
                //createVendorReq.GivenName = carrierDetails.Name;
                //createVendorReq.FamilyName = carrierDetails.Name;
                createVendorReq.CompanyName = carrierDetails.Name;
                createVendorReq.DisplayName = $"EI - {carrierDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Insured)
            {
                InsuredDetails insuredDetails = _PolicyDetailsRepository.GetInsuredDetails(Entity_Id) ?? new InsuredDetails();
                createVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = insuredDetails.Address1,
                    Line2 = insuredDetails.Address2,
                    City = insuredDetails.City,
                    Country = insuredDetails.Country,
                    CountrySubDivisionCode = insuredDetails.State,
                    PostalCode = insuredDetails.ZipCode,
                };
                //createVendorReq.GivenName = insuredDetails.Name;
                //createVendorReq.FamilyName = insuredDetails.Name;
                createVendorReq.CompanyName = insuredDetails.Name;
                createVendorReq.TaxIdentifier = insuredDetails.FEIN;
                createVendorReq.DisplayName = $"EI - {insuredDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Client)
            {
                Clients clients = _ClientsRepository.GetClients(Entity_Id) ?? new Clients();
                createVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = clients.Address,
                    City = clients.City,
                    Country = clients.Country,
                    CountrySubDivisionCode = clients.State,
                    PostalCode = clients.ZipCode,
                };
                //createVendorReq.GivenName = clients.CompanyName;
                //createVendorReq.FamilyName = clients.CompanyName;
                createVendorReq.CompanyName = clients.Name;
                createVendorReq.DisplayName = $"EI - {clients.CompanyName}";
                if (!string.IsNullOrWhiteSpace(clients.Email))
                {
                    createVendorReq.PrimaryEmailAddr = new PrimaryEmailAddr()
                    {
                        Address = clients.Email,
                    };
                }
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
            {
                WholesalerDetails wholesalerDetails = _PolicyDetailsRepository.GetWholesalerDetails(Entity_Id) ?? new WholesalerDetails();
                //createVendorReq.GivenName = wholesalerDetails.Name;
                //createVendorReq.FamilyName = wholesalerDetails.Name;
                createVendorReq.CompanyName = wholesalerDetails.Name;
                createVendorReq.DisplayName = $"EI - {wholesalerDetails.DisplayName}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.State)
            {
                  StateDetails statedetails = new StateDetails();
                SqlTransaction EntityTransaction = null;
                EntityTransaction = baseRepository.GetNewTransaction();                
                if (EntityName != null)
                {
                    EntityName = $"{EntityName}";
                    statedetails = _PolicyDetailsRepository.GetStateDetailsByName(EntityName, Client_Id);
                    if(statedetails == null)
                    {
                        statedetails = new StateDetails();
                        statedetails.Client_Id= Client_Id;
                        statedetails.Name = EntityName;
                        statedetails = _PolicyDetailsRepository.InsertUpdateStateDetails(EntityTransaction, statedetails);
                        EntityTransaction.Commit();
                    }
                }
                
                    
                
                //PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(Entity_Id) ?? new PolicyDetails();
                
                //createVendorReq.GivenName = $"State of {policyDetails.RiskState}";
                //createVendorReq.FamilyName = $"State of {policyDetails.RiskState}";
                createVendorReq.CompanyName =statedetails.Name ;
                createVendorReq.DisplayName = $"EI - {statedetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.SalesRep)
            {
                SalesRepDetails salesRepDetails = _PolicyDetailsRepository.GetSalesRepDetails(Entity_Id) ?? new SalesRepDetails();
                createVendorReq.CompanyName = salesRepDetails.Name;
                createVendorReq.DisplayName = $"EI - {salesRepDetails.Name}";

            }
            else if(EntityType_Id == MasterLookup._EntityTypes.Others)
            {
                OtherEntityDetails otherEntityDetails = _PolicyDetailsRepository.GetOtherEntityDetails(Client_Id, Entity_Id);
                if(otherEntityDetails != null)
                {
                    createVendorReq.CompanyName = otherEntityDetails.Name;
                    createVendorReq.DisplayName = $"EI - {otherEntityDetails.Name}";
                }
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Underwriter)
            {
                UnderWriterDetails underwriterDetails = _PolicyDetailsRepository.GetUnderWriterDetails(Entity_Id) ?? new UnderWriterDetails();
                createVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = underwriterDetails.Address1,
                    Line2 = underwriterDetails.Address2,
                    City = underwriterDetails.City,
                    Country = underwriterDetails.Country,
                    CountrySubDivisionCode = underwriterDetails.State,
                    PostalCode = underwriterDetails.ZipCode,
                };
                createVendorReq.CompanyName = underwriterDetails.Name;
                createVendorReq.DisplayName = $"EI - {underwriterDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.ReferralUnderwriter)
            {
                ReferralUnderwriter referralUnderwriter = _PolicyDetailsRepository.GetReferralUnderwriterDetailsByClientId(Entity_Id, Client_Id);
                if (referralUnderwriter != null)
                {
                    createVendorReq.CompanyName = referralUnderwriter.Name;
                    createVendorReq.DisplayName = $"EI - {referralUnderwriter.Name}";
                }
            }
            return createVendorReq;
        }
        public UpdateVendorReq PrepareUpdateVendorReqObject(ReadByIdRes VendorByIdRes, byte EntityType_Id, long Entity_Id, string EntityName, long Client_Id)
        {
            UpdateVendorReq updateVendorReq = new UpdateVendorReq();
            if(VendorByIdRes != null)
            {
                if(VendorByIdRes.Vendor != null)
                {
                    updateVendorReq.Balance = VendorByIdRes.Vendor.Balance;
                    updateVendorReq.BillRate = VendorByIdRes.Vendor.BillRate;

                    updateVendorReq.Vendor1099 = VendorByIdRes.Vendor.Vendor1099;
                    updateVendorReq.Vendor1099 = VendorByIdRes.Vendor.Vendor1099;
                    updateVendorReq.CurrencyRef = VendorByIdRes.Vendor.CurrencyRef;
                    updateVendorReq.CostRate = VendorByIdRes.Vendor.CostRate;
                    updateVendorReq.domain = VendorByIdRes.Vendor.domain;
                    updateVendorReq.sparse = VendorByIdRes.Vendor.sparse;
                    updateVendorReq.Id = VendorByIdRes.Vendor.Id;
                    updateVendorReq.SyncToken = VendorByIdRes.Vendor.SyncToken;
                    updateVendorReq.Active = VendorByIdRes.Vendor.Active;
                }
            }
            if (EntityType_Id == MasterLookup._EntityTypes.Agency)
            {
                AgentDetails agentDetails = _PolicyDetailsRepository.GetAgentDetails(Entity_Id) ?? new AgentDetails();
                updateVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = agentDetails.Address1,
                    Line2 = agentDetails.Address2,
                    City = agentDetails.City,
                    Country = agentDetails.Country,
                    CountrySubDivisionCode = agentDetails.State,
                    PostalCode = agentDetails.ZipCode,
                };
                //updateVendorReq.GivenName = agentDetails.Name;
                //updateVendorReq.FamilyName = agentDetails.Name;
                updateVendorReq.CompanyName = agentDetails.Name;
                updateVendorReq.TaxIdentifier = agentDetails.FEIN;
                updateVendorReq.DisplayName = $"EI - {agentDetails.Name}";
                if (!string.IsNullOrWhiteSpace(agentDetails.Email))
                {
                    updateVendorReq.PrimaryEmailAddr = new PrimaryEmailAddr()
                    {
                        Address = agentDetails.Email,
                    };
                }
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Carrier)
            {
                CarrierDetails carrierDetails = _PolicyDetailsRepository.GetCarrierDetails(Entity_Id) ?? new CarrierDetails();
                updateVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = carrierDetails.Address1,
                    Line2 = carrierDetails.Address2,
                    City = carrierDetails.City,
                    Country = carrierDetails.Country,
                    CountrySubDivisionCode = carrierDetails.State,
                    PostalCode = carrierDetails.ZipCode,
                };
                //updateVendorReq.GivenName = carrierDetails.Name;
                //updateVendorReq.FamilyName = carrierDetails.Name;
                updateVendorReq.CompanyName = carrierDetails.Name;
                updateVendorReq.DisplayName = $"EI - {carrierDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Insured)
            {
                InsuredDetails insuredDetails = _PolicyDetailsRepository.GetInsuredDetails(Entity_Id) ?? new InsuredDetails();
                updateVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = insuredDetails.Address1,
                    Line2 = insuredDetails.Address2,
                    City = insuredDetails.City,
                    Country = insuredDetails.Country,
                    CountrySubDivisionCode = insuredDetails.State,
                    PostalCode = insuredDetails.ZipCode,
                };
                //updateVendorReq.GivenName = insuredDetails.Name;
                //updateVendorReq.FamilyName = insuredDetails.Name;
                updateVendorReq.CompanyName = insuredDetails.Name;
                updateVendorReq.TaxIdentifier = insuredDetails.FEIN;
                updateVendorReq.DisplayName = $"EI - {insuredDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Client)
            {
                Clients clients = _ClientsRepository.GetClients(Entity_Id) ?? new Clients();
                updateVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = clients.Address,
                    City = clients.City,
                    Country = clients.Country,
                    CountrySubDivisionCode = clients.State,
                    PostalCode = clients.ZipCode,
                };
                //updateVendorReq.GivenName = clients.CompanyName;
                //updateVendorReq.FamilyName = clients.CompanyName;
                updateVendorReq.CompanyName = clients.CompanyName;
                updateVendorReq.DisplayName = $"EI - {clients.CompanyName}";
                if (!string.IsNullOrWhiteSpace(clients.Email))
                {
                    updateVendorReq.PrimaryEmailAddr = new PrimaryEmailAddr()
                    {
                        Address = clients.Email,
                    };
                }
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
            {
                WholesalerDetails wholesalerDetails = _PolicyDetailsRepository.GetWholesalerDetails(Entity_Id) ?? new WholesalerDetails();
                //updateVendorReq.GivenName = wholesalerDetails.Name;
                //updateVendorReq.FamilyName = wholesalerDetails.Name;
                updateVendorReq.CompanyName = wholesalerDetails.Name;
                updateVendorReq.DisplayName = $"EI - {wholesalerDetails.DisplayName}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.State)
            {
                StateDetails statedetails = new StateDetails();
                SqlTransaction EntityTransaction = null;
                EntityTransaction = baseRepository.GetNewTransaction();
                if (EntityName != null)
                {
                    statedetails = _PolicyDetailsRepository.GetStateDetailsByName(EntityName, Client_Id);
                    if (statedetails == null)
                    {
                        statedetails = new StateDetails();
                        statedetails.Client_Id = Client_Id;
                        statedetails.Name = EntityName;
                        statedetails = _PolicyDetailsRepository.InsertUpdateStateDetails(EntityTransaction, statedetails);
                        EntityTransaction.Commit();
                    }
                }



                //PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(Entity_Id) ?? new PolicyDetails();

                //createVendorReq.GivenName = $"State of {policyDetails.RiskState}";
                //createVendorReq.FamilyName = $"State of {policyDetails.RiskState}";
                updateVendorReq.CompanyName = statedetails.Name;
                updateVendorReq.DisplayName = $"EI - {statedetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.SalesRep)
            {
                SalesRepDetails salesRepDetails = _PolicyDetailsRepository.GetSalesRepDetails(Entity_Id) ?? new SalesRepDetails();
                updateVendorReq.CompanyName = salesRepDetails.Name;
                updateVendorReq.DisplayName = $"EI - {salesRepDetails.Name}";
                
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Others)
            {
                OtherEntityDetails otherEntityDetails = _PolicyDetailsRepository.GetOtherEntityDetails(Client_Id, Entity_Id);
                if (otherEntityDetails != null)
                {
                    updateVendorReq.CompanyName = otherEntityDetails.Name;
                    updateVendorReq.DisplayName = $"EI - {otherEntityDetails.Name}";
                }
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.Underwriter)
            {
                UnderWriterDetails underwriterDetails = _PolicyDetailsRepository.GetUnderWriterDetails(Entity_Id) ?? new UnderWriterDetails();
                updateVendorReq.BillAddr = new BillAddr()
                {
                    Line1 = underwriterDetails.Address1,
                    Line2 = underwriterDetails.Address2,
                    City = underwriterDetails.City,
                    Country = underwriterDetails.Country,
                    CountrySubDivisionCode = underwriterDetails.State,
                    PostalCode = underwriterDetails.ZipCode,
                };
                updateVendorReq.CompanyName = underwriterDetails.Name;
                updateVendorReq.DisplayName = $"EI - {underwriterDetails.Name}";
            }
            else if (EntityType_Id == MasterLookup._EntityTypes.ReferralUnderwriter)
            {
                ReferralUnderwriter referralUnderwriter = _PolicyDetailsRepository.GetReferralUnderwriterDetailsByClientId(Entity_Id, Client_Id);
                if (referralUnderwriter != null)
                {
                    updateVendorReq.CompanyName = referralUnderwriter.Name;
                    updateVendorReq.DisplayName = $"EI - {referralUnderwriter.Name}";
                }
            }
            return updateVendorReq;
        }
    }
}
