using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.EI;
using Accounting.Entities.External.ePay;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class PolicyDetailsRepository : IPolicyDetailsRepository
    {
        public PolicyDetails InsertUpdatePolicyDetails(SqlTransaction sqlTransaction, PolicyDetails policyDetails)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@Id", Value = policyDetails.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Client_Id",policyDetails.Client_Id),
                    new SqlParameter("@Submission_Id",policyDetails.Submission_Id),
                    new SqlParameter("@PolicyNumber",policyDetails.PolicyNumber),
                    new SqlParameter("@Corporation",policyDetails.Corporation),
                    new SqlParameter("@Location",policyDetails.Location),
                    new SqlParameter("@RiskState",policyDetails.RiskState),
                    new SqlParameter("@PaymentPlan",policyDetails.PaymentPlan),
                    new SqlParameter("@BillType",policyDetails.BillType),
                    new SqlParameter("@EffectiveDate",policyDetails.EffectiveDate),
                    new SqlParameter("@ExpirationDate",policyDetails.ExpirationDate),
                    new SqlParameter("@CancelDate",policyDetails.CancelDate),
                    new SqlParameter("@EndorsementEffectiveDate",policyDetails.EndorsementEffectiveDate),
                    new SqlParameter("@PolicyAction",policyDetails.PolicyAction),
                    new SqlParameter("@TransactionType",policyDetails.TransactionType),
                    new SqlParameter("@PolicyDescription",policyDetails.PolicyDescription),
                    new SqlParameter("@InvoiceNote",policyDetails.InvoiceNote),
                    new SqlParameter("@MasterPolicy",policyDetails.MasterPolicy),
                    new SqlParameter("@LoanNum",policyDetails.LoanNum),
                    new SqlParameter("@PostAsShell",policyDetails.PostAsShell),
                     new SqlParameter("@CustomerType",policyDetails.CustomerType),
                     new SqlParameter("@EndorsementNumber",policyDetails.EndorsementNumber),
                    new SqlParameter("@Quote_Id",policyDetails.Quote_Id),
                    new SqlParameter("@WholesalerDetail_Id",policyDetails.WholesalerDetail_Id),
                    new SqlParameter("@UnderWriterDetail_Id",policyDetails.UnderWriterDetail_Id),
                    new SqlParameter("@InterestDetail_Id",policyDetails.InterestDetail_Id),
                    new SqlParameter("@InsuredDetail_Id",policyDetails.InsuredDetail_Id),
                    new SqlParameter("@FinanceDetail_Id",policyDetails.FinanceDetail_Id),
                    new SqlParameter("@CarrierDetail_Id",policyDetails.CarrierDetail_Id),
                    new SqlParameter("@AgentDetail_Id",policyDetails.AgentDetail_Id),
                    new SqlParameter("@Source_Id",policyDetails.Source_Id),
                    new SqlParameter("@OriginalSubmission_Id",policyDetails.OriginalSubmission_Id),
                    new SqlParameter("@PreviousSubmission_Id",policyDetails.PreviousSubmission_Id),
                    new SqlParameter("@ReferralUnderwriter_Id",policyDetails.ReferralUnderwriter_Id),
                    new SqlParameter("@MortgageeDetail_Id",policyDetails.MortgageeDetail_Id),
                    new SqlParameter("@AgentBrokerContactDetail_Id", policyDetails.AgentBrokerContactDetail_Id)

                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePolicyDetails", spParams);
            policyDetails.Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            return policyDetails;
        }
        public PolicyDetailsLineItems InsertUpdatePolicyDetailsLineItems(SqlTransaction sqlTransaction, PolicyDetailsLineItems policyDetailsLineItems)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@Id", Value = policyDetailsLineItems.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@PolicyDetails_Id",policyDetailsLineItems.PolicyDetails_Id),
                    new SqlParameter("@Submission_Id",policyDetailsLineItems.Submission_Id),
                    new SqlParameter("@LineType",policyDetailsLineItems.LineType),
                    new SqlParameter("@HouseCommission",policyDetailsLineItems.HouseCommission),
                    new SqlParameter("@AgentCommission",policyDetailsLineItems.AgentCommission),
                     new SqlParameter("@CommissionAmount",policyDetailsLineItems.CommissionAmount),
                    new SqlParameter("@BrokerCommissionAmount",policyDetailsLineItems.BrokerCommissionAmount),
                    new SqlParameter("@Coverage",policyDetailsLineItems.Coverage),
                    new SqlParameter("@CustomerType", policyDetailsLineItems.CustomerType),

                    new SqlParameter("@PolicyDetailDescription",policyDetailsLineItems.PolicyDetailDescription),
                    new SqlParameter("@FeeTaxAmount",policyDetailsLineItems.FeeTaxAmount),
                    new SqlParameter("@Premium",policyDetailsLineItems.Premium),
                    new SqlParameter("@FeeTaxable",policyDetailsLineItems.FeeTaxable),
                    new SqlParameter("@EffectiveDate",policyDetailsLineItems.EffectiveDate),
                    new SqlParameter("@State",policyDetailsLineItems.State),
                    new SqlParameter("@Is_Commissionable",policyDetailsLineItems.Is_Commissionable),
                    new SqlParameter("@Client_Id", policyDetailsLineItems.Client_Id),
                    new SqlParameter("@MinPremium",policyDetailsLineItems.MinPremium)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePolicyDetailsLineItems", spParams);
            policyDetailsLineItems.Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            return policyDetailsLineItems;
        }
        public UnderWriterDetails InsertUpdateUnderWriterDetails(SqlTransaction sqlTransaction, UnderWriterDetails underWriterDetails)
        {
            SqlParameter[] spParams =
              {
                new SqlParameter(){ParameterName="@UnderWriterDetail_Id", Value = underWriterDetails.UnderWriterDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@UWReference_Id",underWriterDetails.UWReference_Id),
                    new SqlParameter("@Name",underWriterDetails.Name),
                    new SqlParameter("@Address1",underWriterDetails.Address1),
                    new SqlParameter("@Address2",underWriterDetails.Address2),
                    new SqlParameter("@City",underWriterDetails.City),
                    new SqlParameter("@Country",underWriterDetails.Country),
                    new SqlParameter("@County",underWriterDetails.County),
                    new SqlParameter("@State",underWriterDetails.State),
                    new SqlParameter("@ZipCode",underWriterDetails.ZipCode),
                    new SqlParameter("@Client_Id",underWriterDetails.Client_Id),
                    new SqlParameter("@AssignUserName",underWriterDetails.AssignUserName),
                    new SqlParameter("@AssignUser_Id", underWriterDetails.AssignUser_Id)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateUnderWriterDetails", spParams);
            underWriterDetails.UnderWriterDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@UnderWriterDetail_Id").FirstOrDefault().Value);
            return underWriterDetails;
        }
        public AgentDetails InsertUpdateAgentDetails(SqlTransaction sqlTransaction, AgentDetails agentDetails)
        {
            SqlParameter[] value = [
                new() { ParameterName = "@AgentDetail_Id", Value = agentDetails.AgentDetail_Id, Direction = ParameterDirection.InputOutput },
                new SqlParameter("@AgentReference_Id", agentDetails.AgentReference_Id),
                new SqlParameter("@Name", agentDetails.Name),
                new SqlParameter("@Address1", agentDetails.Address1),
                new SqlParameter("@Address2", agentDetails.Address2),
                new SqlParameter("@City", agentDetails.City),
                new SqlParameter("@Country", agentDetails.Country),
                new SqlParameter("@County", agentDetails.County),
                new SqlParameter("@State", agentDetails.State),
                new SqlParameter("@ZipCode", agentDetails.ZipCode),
                new SqlParameter("@Client_Id", agentDetails.Client_Id),
                new SqlParameter("@Email", agentDetails.Email),
                new SqlParameter("@FEIN", agentDetails.FEIN),
                new SqlParameter("@EI_Accounting_Id", agentDetails.EI_Accounting_Id),
                new SqlParameter("@AccountNumber", agentDetails.AgentAccountNumber),
                new SqlParameter("@RoutingNumber", agentDetails.AgentRoutingNumber),
                new SqlParameter("@BankName", agentDetails.BankName),
                new SqlParameter("@AccountTypeName", agentDetails.AccountTypeName),
                new SqlParameter("@SalesRep", agentDetails.SalesRep),
                new SqlParameter("@ProducerType", agentDetails.ProducerType),
                new SqlParameter("@AssignedSalesRep_Id", agentDetails.AssignedSalesRep_Id),
                new SqlParameter("@ParentAgencyDetails_Id", agentDetails.ParentAgencyDetails_Id),
                new SqlParameter("@DBA", agentDetails.DBA),
                new SqlParameter("@AgreementDate", agentDetails.AgreementDate),
                new SqlParameter("@CompanyType", agentDetails.CompanyType),
                new SqlParameter("@AgentStatus", agentDetails.AgentStatus),
                new SqlParameter("@Phone", agentDetails.Phone),
            ];
            SqlParameter[] spParams =
              value;
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateAgentDetails", spParams);
            agentDetails.AgentDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@AgentDetail_Id").FirstOrDefault().Value);
            return agentDetails;
        }
        public CarrierDetails InsertUpdateCarrierDetails(SqlTransaction sqlTransaction, CarrierDetails carrierDetails)
        {
            SqlParameter[] spParams =
              {
                new SqlParameter(){ParameterName="@CarrierDetail_Id", Value = carrierDetails.CarrierDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@CarrierReference_Id",carrierDetails.CarrierReference_Id),
                    new SqlParameter("@Name",carrierDetails.Name),
                    new SqlParameter("@Address1",carrierDetails.Address1),
                    new SqlParameter("@Address2",carrierDetails.Address2),
                    new SqlParameter("@City",carrierDetails.City),
                    new SqlParameter("@Country",carrierDetails.Country),
                    new SqlParameter("@County",carrierDetails.County),
                    new SqlParameter("@State",carrierDetails.State),
                    new SqlParameter("@ZipCode",carrierDetails.ZipCode),
                    new SqlParameter("@CarrierStatus",carrierDetails.CarrierStatus),
                    new SqlParameter("@NAIC", carrierDetails.NAIC),
                    new SqlParameter("@Client_Id",carrierDetails.Client_Id),
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateCarrierDetails", spParams);
            carrierDetails.CarrierDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@CarrierDetail_Id").FirstOrDefault().Value);
            return carrierDetails;
        }
        public FinanceDetails InsertUpdateFinanceDetails(SqlTransaction sqlTransaction, FinanceDetails financeDetails)
        {
            SqlParameter[] spParams =
              {
                new SqlParameter(){ParameterName="@FinanceDetail_Id", Value = financeDetails.FinanceDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@FinanceReference_Id",financeDetails.FinanceReference_Id),
                    new SqlParameter("@Name",financeDetails.Name),
                    new SqlParameter("@Address1",financeDetails.Address1),
                    new SqlParameter("@Address2",financeDetails.Address2),
                    new SqlParameter("@City",financeDetails.City),
                    new SqlParameter("@Country",financeDetails.Country),
                    new SqlParameter("@County",financeDetails.County),
                    new SqlParameter("@State",financeDetails.State),
                    new SqlParameter("@ZipCode",financeDetails.ZipCode),
                    new SqlParameter("@Client_Id",financeDetails.Client_Id),
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateFinanceDetails", spParams);
            financeDetails.FinanceDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@FinanceDetail_Id").FirstOrDefault().Value);
            return financeDetails;
        }
        public InsuredDetails InsertUpdateInsuredDetails(SqlTransaction sqlTransaction, InsuredDetails insuredDetails)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@InsuredDetail_Id", Value = insuredDetails.InsuredDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@InsuredReference_Id",insuredDetails.InsuredReference_Id),
                    new SqlParameter("@Name",insuredDetails.Name),
                    new SqlParameter("@Address1",insuredDetails.Address1),
                    new SqlParameter("@Address2",insuredDetails.Address2),
                    new SqlParameter("@City",insuredDetails.City),
                    new SqlParameter("@Country",insuredDetails.Country),
                    new SqlParameter("@County",insuredDetails.County),
                    new SqlParameter("@State",insuredDetails.State),
                    new SqlParameter("@Physical_Address1",insuredDetails.Physical_Address1),
                    new SqlParameter("@Physical_Address2",insuredDetails.Physical_Address2),
                    new SqlParameter("@Physical_City",insuredDetails.Physical_City),
                    new SqlParameter("@Physical_Country",insuredDetails.Physical_Country),
                    new SqlParameter("@Physical_State",insuredDetails.Physical_State),
                    new SqlParameter("@Physical_ZipCode",insuredDetails.Physical_ZipCode),
                    new SqlParameter("@ZipCode",insuredDetails.ZipCode),
                    new SqlParameter("@Client_Id",insuredDetails.Client_Id),
                    new SqlParameter("@FEIN",insuredDetails.FEIN),
                    new SqlParameter("@DBA", insuredDetails.InsuredDBA),
                    new SqlParameter("@BusinessDescription", insuredDetails.BusinessDescription),
                    new SqlParameter("@Email", insuredDetails.Email),
                    new SqlParameter("@Phone", insuredDetails.Phone),
                    new SqlParameter("@ReferralSource",insuredDetails.ReferralSource)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateInsuredDetails", spParams);
            insuredDetails.InsuredDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@InsuredDetail_Id").FirstOrDefault().Value);
            return insuredDetails;
        }
        public InterestDetails InsertUpdateInterestDetails(SqlTransaction sqlTransaction, InterestDetails interestDetails)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@InterestDetail_Id", Value = interestDetails.InterestDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@InterestReference_Id",interestDetails.InterestReference_Id),
                    new SqlParameter("@Name",interestDetails.Name),
                    new SqlParameter("@Address1",interestDetails.Address1),
                    new SqlParameter("@Address2",interestDetails.Address2),
                    new SqlParameter("@City",interestDetails.City),
                    new SqlParameter("@Country",interestDetails.Country),
                    new SqlParameter("@County",interestDetails.County),
                    new SqlParameter("@State",interestDetails.State),
                    new SqlParameter("@ZipCode",interestDetails.ZipCode),
                    new SqlParameter("@Client_Id",interestDetails.Client_Id),

                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateInterestDetails", spParams);
            interestDetails.InterestDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@InterestDetail_Id").FirstOrDefault().Value);
            return interestDetails;
        }
        public MortgageeDetails InsertUpdateMortgageeDetails(SqlTransaction sqlTransaction, MortgageeDetails Details)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@MortgageeDetail_Id", Value = Details.MortgageeDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@MortgageeReference_Id",Details.MortgageeReference_Id),
                    new SqlParameter("@MortgageeType_Id",Details.MortgageeType_Id),
                    new SqlParameter("@Name",Details.Name),
                    new SqlParameter("@Address1",Details.Address1),
                    new SqlParameter("@Address2",Details.Address2),
                    new SqlParameter("@City",Details.City),
                    new SqlParameter("@Country",Details.Country),
                    new SqlParameter("@County",Details.County),
                    new SqlParameter("@State",Details.State),
                    new SqlParameter("@ZipCode",Details.ZipCode),
                    new SqlParameter("@Client_Id",Details.Client_Id),
                    new SqlParameter("@CreatedByOrUpdatedBy",Details.CreatedBy),
                    new SqlParameter("@Email", Details.Email)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateMortgageeDetails", spParams);
            Details.MortgageeDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@MortgageeDetail_Id").FirstOrDefault().Value);
            return Details;
        }
        public WholesalerDetails InsertUpdateWholesalerDetails(SqlTransaction sqlTransaction, WholesalerDetails wholesalerDetails)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName="@WholesalerDetail_Id", Value = wholesalerDetails.WholesalerDetail_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@WholesalerReference_Id", wholesalerDetails.WholesalerReference_Id),
                new SqlParameter("@Name", wholesalerDetails.Name),
                new SqlParameter("@DisplayName", wholesalerDetails.DisplayName),
                new SqlParameter("@ParentID", wholesalerDetails.ParentID),
                new SqlParameter("@IsActive", wholesalerDetails.IsActive),
                new SqlParameter("@AlternateID", wholesalerDetails.AlternateID),
                new SqlParameter("@Client_Id", wholesalerDetails.Client_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateWholesalerDetails", sqlParameters);
            wholesalerDetails.WholesalerDetail_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@WholesalerDetail_Id").FirstOrDefault().Value);
            return wholesalerDetails;
        }
        public StateDetails InsertUpdateStateDetails(SqlTransaction sqlTransaction, StateDetails stateDetails)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName="@StateDetail_Id", Value = stateDetails.StateDetail_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id",stateDetails.Client_Id),
                new SqlParameter("@Name",stateDetails.Name),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertStateDetails", sqlParameters);
            stateDetails.StateDetail_Id = Convert.ToInt32(sqlParameters.Where(c => c.ParameterName == "@StateDetail_Id").FirstOrDefault().Value);
            return stateDetails;
        }
        public StateDetails GetStateDetailsByName(string Name, long Client_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@Client_Id",Client_Id),
                    new SqlParameter("@Name",Name)
                };
            return SQLHelper.GetObject(new StateDetails(), "USP_Accounting_GetStateDetailByName", spParams);
        }


        public PolicyDetails GetPolicyDetails(long Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@Id",Id),
                };
            return SQLHelper.GetObject(new PolicyDetails(), "USP_Accounting_GetPolicyDetails", spParams);
        }
        public List<PolicyDetailsLineItems> GetPolicyDetailsLineItems(long PolicyDetails_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
                };
            return SQLHelper.GetObjectList(new PolicyDetailsLineItems(), "USP_Accounting_GetPolicyDetailsLineItems", spParams);
        }
        public AgentDetails GetAgentDetails(long AgentDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@AgentDetail_Id",AgentDetail_Id),
                };
            return SQLHelper.GetObject(new AgentDetails(), "USP_Accounting_GetAgentDetails", spParams);
        }
        public CarrierDetails GetCarrierDetails(long CarrierDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@CarrierDetail_Id",CarrierDetail_Id),
                };
            return SQLHelper.GetObject(new CarrierDetails(), "USP_Accounting_GetCarrierDetails", spParams);
        }
        public InsuredDetails GetInsuredDetails(long InsuredDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@InsuredDetail_Id",InsuredDetail_Id),
                      };
            return SQLHelper.GetObject(new InsuredDetails(), "USP_Accounting_GetInsuredDetails", spParams);
        }
        public InterestDetails GetInterestDetails(long InterestDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@InterestDetail_Id",InterestDetail_Id),
                      };
            return SQLHelper.GetObject(new InterestDetails(), "USP_Accounting_GetInterestDetails", spParams);
        }
        public MortgageeDetails GetMortgageeDetails(long MortgageeDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@MortgageeDetail_Id",MortgageeDetail_Id),
                      };
            return SQLHelper.GetObject(new MortgageeDetails(), "USP_Accounting_GetMortgageeDetails", spParams);
        }
        public WholesalerDetails GetWholesalerDetails(long WholesalerDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@WholesalerDetail_Id",WholesalerDetail_Id),
                };
            return SQLHelper.GetObject(new WholesalerDetails(), "USP_Accounting_GetWholesalerDetails", spParams);
        }
        public UnderWriterDetails GetUnderWriterDetails(long UnderWriterDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@UnderWriterDetail_Id",UnderWriterDetail_Id),
                };
            return SQLHelper.GetObject(new UnderWriterDetails(), "USP_Accounting_GetUnderWriterDetails", spParams);
        }
        public FinanceDetails GetFinanceDetails(long FinanceDetail_Id)
        {
            SqlParameter[] spParams =
                      {
                    new SqlParameter("@FinanceDetail_Id",FinanceDetail_Id),
                };
            return SQLHelper.GetObject(new FinanceDetails(), "USP_Accounting_GetFinanceDetails", spParams);
        }
        public PolicyDetails GetPolicyDetailsBySubmission_Id(long Submission_Id, long Client_Id)
        {
            SqlParameter[] spParams =
            {
              new SqlParameter("@Submission_Id",Submission_Id),
              new SqlParameter("@Client_Id",Client_Id),
            };
            return SQLHelper.GetObject(new PolicyDetails(), "USP_Accounting_GetPolicyDetailsBySubmission_Id", spParams);
        }
        public void InactivePolicyDetailsLineItems(SqlTransaction sqlTransaction, long PolicyDetails_Id, long client_Id)
        {
            SqlParameter[] spParams =
            {
              new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
              new SqlParameter("@Client_Id",client_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactivePolicyDetailsLineItems", spParams);
        }
        public List<PolicyDetails> GetAllPolicyDetails(PolicyDetails policySearch, long Client_Id, int PageSize, int PageNumber, ref long TotalRecords)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Client_Id", Client_Id),
                 new SqlParameter("@InsuredName",policySearch.InsuredName),
                new SqlParameter("@PolicyNumber",policySearch.PolicyNumber),
                new SqlParameter("@Submission_Id",policySearch.Submission_Id == 0? null : policySearch.Submission_Id),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@AgencyName",policySearch.AgencyName),
                new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
            };
            var result = SQLHelper.GetObjectList(new PolicyDetails(), "USP_Accounting_GetAllPolicyDetails_V1", sqlParameter);
            TotalRecords = Convert.ToInt64(sqlParameter.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return result;
        }

        public List<LookupEntity> GetAllAgentDetailsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllAgentDetailsByName", sqlParameter);
        }
        public List<LookupEntity> GetAllInsuredsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllInsuredsByName", sqlParameter);
        }
        public List<LookupEntity> GetAllClientsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllClientsByName", sqlParameter);
        }
        public List<LookupEntity> GetAllCarriersByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllCarriersByName", sqlParameter);
        }
        //public List<ARList> GetARSearchResults(Search search)
        //{
        //    SqlParameter[] sqlParameter =
        //    {
        //        new SqlParameter("@Name", search.Name),
        //        new SqlParameter("@Client_Id", search.Client_Id),
        //        new SqlParameter("@EntityType_Id", search.EntityType_Id),
        //       // new SqlParameter("@Direction_Id", search.Direction_Id),
        //    };
        //    return SQLHelper.GetObjectList(new ARList(), "USP_Accounting_GetARSearchResults", sqlParameter);
        //}
        public List<InvoiceDetails> GetARSearchResults(Search search, int pageIndex, int pageSize, string SortOrder)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@EntityType_Id", search.EntityType_Id),
                new SqlParameter("@Entity_Id", search.Entity_Id),
                new SqlParameter("@Entity_Name", search.Name),
                new SqlParameter("@LineItem_Id", search.LineItem_Id),
                new SqlParameter("@LineItemIds", search.LineItemList),
                new SqlParameter("@PolicyNumber", search.PolicyNumber),
                new SqlParameter("@AgencyName", search.AgencyName),
                new SqlParameter("@InsuredName", search.InsuredName),
                new SqlParameter("@OutstandingBalances", search.OutstandingBalances),
                new SqlParameter("@InvoiceNo", search.InvoiceNo),
                new SqlParameter("@Client_Id", search.Client_Id),
                new SqlParameter("@BillingType_Id",search.BillingType_Id),
                new SqlParameter("SortOrder",SortOrder),
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize),
            };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetARSearchResults_V7", sqlParameter);
        }
        public List<InvoiceDetails> GetAPSearchResults(Search search, int pageIndex, int pageSize, string SortOrder)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@EntityType_Id", search.EntityType_Id),
                new SqlParameter("@Entity_Id", search.Entity_Id),
                new SqlParameter("@LineItem_Id", search.LineItem_Id),
                new SqlParameter("@LineItemIds", search.LineItemList),
                new SqlParameter("@PolicyNumber", search.PolicyNumber),
                new SqlParameter("@AgencyName", search.AgencyName),
                new SqlParameter("@InsuredName", search.InsuredName),
                new SqlParameter("@OutstandingBalances", search.OutstandingBalances),
                new SqlParameter("@InvoiceNo", search.InvoiceNo),
                new SqlParameter("@EntityName",search.EntityName),
                new SqlParameter("@Client_Id", search.Client_Id),
                new SqlParameter("@BillingType_Id",search.BillingType_Id),
                new SqlParameter("@SortOrder",SortOrder),
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize),
            };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetAPSearchResults_V7", sqlParameter);
        }
        public List<LookupEntity> GetAllWholesalersByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllWholesalersByName", sqlParameter);
        }
        public List<LookupEntity> GetAllSalesRepsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllSalesRepsByName", sqlParameter);
        }
        public List<LookupEntity> GetAllStateDetailsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllStateDetailsByName", sqlParameters);
        }
        public List<LookupEntity> GetAllFinanceDetailsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllFinanceDetailsByName", sqlParameters);
        }
        public List<InvoiceDetails> GetSearchInvoicesPaymentDetails(string InvoiceorPolicyNo, byte Direction_Id, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@InvoiceorPolicyNo", InvoiceorPolicyNo),
                new SqlParameter("@Direction_Id", Direction_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetARAPSearchInvoicesPaymentDetails", sqlParameter);
        }
        public TransactionDetails GetTransactionDetails(long Quote_Id, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Quote_Id", Quote_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObject(new TransactionDetails(), "USP_Accounting_GetTransactionDetails", sqlParameter);
        }
        public List<InvoiceList> GetAllOpenInvoices(long Client_Id, long Entity_Id, byte EntityType_Id, string querySearch)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Entity_Id", Entity_Id),
                new SqlParameter("@EntityType_Id", EntityType_Id),
                new SqlParameter("@QuerySearch", querySearch),
            };
            return SQLHelper.GetObjectList(new InvoiceList(), "USP_Accounting_GetAllOpenInvoices_V11", sqlParameter);
        }
        public EntityInfo GetEntityInfo(long Client_Id, long Entity_Id, byte EntityType_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Entity_Id", Entity_Id),
                new SqlParameter("@EntityType_Id", EntityType_Id),
            };
            return SQLHelper.GetObject(new EntityInfo(), "USP_Accounting_GetEntityInformationByEntity_Id", sqlParameter);
        }
        public AgentDetails GetAgentDetailsByReference_Id(long Client_Id, string Reference_Id)
        {
            SqlParameter[] sqlParameter =
            {
         new SqlParameter("@Client_Id", Client_Id),
         new SqlParameter("@Reference_Id", Reference_Id),
     };
            return SQLHelper.GetObject(new AgentDetails(), "USP_Accounting_GetAgentDetailsByReference_Id", sqlParameter);
        }
        public InsuredDetails GetInsuredDetailsByReference_Id(long Client_Id, string Reference_Id)
        {
            SqlParameter[] sqlParameter =
            {
         new SqlParameter("@Client_Id", Client_Id),
         new SqlParameter("@Reference_Id", Reference_Id),
     };
            return SQLHelper.GetObject(new InsuredDetails(), "USP_Accounting_GetInsuredDetailsByReference_Id", sqlParameter);
        }
        public CarrierDetails GetCarrierDetailsByReference_Id(long Client_Id, string Reference_Id)
        {
            SqlParameter[] sqlParameter =
            {
         new SqlParameter("@Client_Id", Client_Id),
         new SqlParameter("@Reference_Id", Reference_Id),
     };
            return SQLHelper.GetObject(new CarrierDetails(), "USP_Accounting_GetCarrierDetailsByReference_Id", sqlParameter);
        }
        public WholesalerDetails GetWholesalerDetailsByReference_Id(long Client_Id, string Reference_Id)
        {
            SqlParameter[] sqlParameter =
            {
         new SqlParameter("@Client_Id", Client_Id),
         new SqlParameter("@Reference_Id", Reference_Id),
     };
            return SQLHelper.GetObject(new WholesalerDetails(), "USP_Accounting_GetWholesalerDetailsByReference_Id", sqlParameter);
        }
        public int InactivePolicyDetails(SqlTransaction sqlTransaction, long PolicyDetails_Id, long client_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
                new SqlParameter("@Client_Id",client_Id),
            };
            return SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactivePolicyDetails", spParams);
       }
        public void UpdatePolicyNumberByPreviousSubmissionID(long Submission_Id, string PolicyNumber, long Client_Id, long OriginalSubmission_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Submission_Id",Submission_Id),
                new SqlParameter("@PolicyNumber",PolicyNumber),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@OriginalSubmission_Id", OriginalSubmission_Id)
            };
            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_UpdatePolicyNumberByPreviousSubmissionID", sp);
        }
        public void UpdatePolicyNumberByPreviousSubmissionIDWithTransaction(SqlTransaction sqlTransaction, long Submission_Id, string PolicyNumber, long Client_Id, long OriginalSubmission_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Submission_Id",Submission_Id),
                new SqlParameter("@PolicyNumber",PolicyNumber),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@OriginalSubmission_Id", OriginalSubmission_Id)
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdatePolicyNumberByPreviousSubmissionID", sp);
        }
        public void updatePayeerAndEntityByInvoiceDetailId(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@InvoiceDetail_Id",invoiceDetails.Id),
                new SqlParameter("@Entity_Id",invoiceDetails.Entity_Id),
                new SqlParameter("@EntityType_Id", invoiceDetails.EntityType_Id),
            };
            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_UpdatePayeerAndEntityByInvoiceDetailId", sp);
        }
        public InvoiceDetails InsertUpdateOtherEntityDetails(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails, long Client_Id)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@Id", Value = invoiceDetails.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Name",invoiceDetails.EntityName),
                    new SqlParameter("@Entity_Id",invoiceDetails.Entity_Id),
                    new SqlParameter("@EntityType_Id",invoiceDetails.EntityType_Id),
                    new SqlParameter("@Client_Id",Client_Id)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateOtherEntityDetails", spParams);
            invoiceDetails.Entity_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            return invoiceDetails;
        }
        public List<LookupEntity> GetAllOtherEnityDetailsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllOtherEntityDetailsByName", sqlParameters);
        }
        public SalesRepDetails InsertUpdateSalesRepDetails(SqlTransaction sqlTransaction, SalesRepDetails salesRepDetails)
        {
            SqlParameter[] sqlParameters = {
                new() { ParameterName = "@SalesRepDetails_Id", Value = salesRepDetails.Id, Direction = ParameterDirection.InputOutput },
                 new SqlParameter("@Name", salesRepDetails.Name),
                new SqlParameter("@Client_Id", salesRepDetails.Client_Id),
                new SqlParameter("@SalesRepReference_Id", salesRepDetails.SalesRepReference_Id),

            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateSalesRepDetails", sqlParameters);
            salesRepDetails.Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@SalesRepDetails_Id").FirstOrDefault().Value);
            return salesRepDetails;
        }
        public InvoiceDetails GetEntityNameByEntityTypeIdAndInvoiceId(long Client_Id, long InvoiceId, byte EntityType_Id)
        {
            SqlParameter[] sqlParameter =
            {
         new SqlParameter("@Client_Id", Client_Id),
         new SqlParameter("@Invoice_Id",InvoiceId),
         new SqlParameter("@EntityType_Id", EntityType_Id)
     };
            return SQLHelper.GetObject(new InvoiceDetails(), "USP_Accounting_GetDefaultEntityByInvoiceIdAndEntityTypeId", sqlParameter);
        }
        public SalesRepDetails GetSalesRepDetails(long Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Id",Id)
            };
            return SQLHelper.GetObject(new SalesRepDetails(), "USP_Accounting_GetSalesRepDetails", sqlParameters);
        }
        public InvoiceDetails InsertUpdateSalesRepDetails(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails, long Client_Id)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@SalesRepDetails_Id", Value = invoiceDetails.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Name",invoiceDetails.EntityName),
                    new SqlParameter("@EntityType_Id",invoiceDetails.EntityType_Id),
                    new SqlParameter("@Client_Id",Client_Id)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateSalesRepDetails", spParams);
            invoiceDetails.Entity_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@SalesRepDetails_Id").FirstOrDefault().Value);
            return invoiceDetails;
        }
        public void UpdateAssignedSalesRepIdByInvoiceIdandEntityId(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Invoice_Id",invoiceDetails.Invoice_Id),
                new SqlParameter("@Entity_Id",invoiceDetails.Entity_Id)
            };
            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_UpdateAssignedSalesRepIdByInvoiceIdandEntityId", sp);
        }
        public CarrierDetails InsertCarrierDetails(CarrierDetails carrierDetails)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@CarrierDetail_Id", Value = carrierDetails.CarrierDetail_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id",carrierDetails.Client_Id),
                new SqlParameter("@CarrierReference_Id",carrierDetails.CarrierReference_Id),
                new SqlParameter("@Name",carrierDetails.Name),
                new SqlParameter("@Address1",carrierDetails.Address1),
                new SqlParameter("@City",carrierDetails.City),
                new SqlParameter("@State",carrierDetails.State),
                new SqlParameter("@ZipCode",carrierDetails.ZipCode),
                new SqlParameter("@CarrierStatus",carrierDetails.CarrierStatus)
            };

            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_InsertCarrierDetails", sp);
            carrierDetails.CarrierDetail_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@CarrierDetail_Id").FirstOrDefault().Value);
            return carrierDetails;

        }
        public List<LookupEntity> GetAllUnderwritersByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllUnderwritersByName", sqlParameter);
        }        
        public OtherEntityDetails GetOtherEntityDetails(long Client_Id,long Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Id",Id)
            };
            return SQLHelper.GetObject(new OtherEntityDetails(), "USP_Accounting_GetOtherEntityDetailsByClientId", sqlParameters);
        }
        public ParentAgentDetails InsertUpdateParentAgentDetails(SqlTransaction sqlTransaction, ParentAgentDetails parentAgentDetails)
        {
            SqlParameter[] value = [
                new() { ParameterName = "@ParentAgencyDetails_Id", Value = parentAgentDetails.ParentAgentDetail_Id, Direction = ParameterDirection.InputOutput },
                new SqlParameter("@AgentReference_Id", parentAgentDetails.AgentReference_Id),
                new SqlParameter("@Name", parentAgentDetails.Name),
                new SqlParameter("@Address1", parentAgentDetails.Address1),
                new SqlParameter("@Address2", parentAgentDetails.Address2),
                new SqlParameter("@City", parentAgentDetails.City),
                new SqlParameter("@Country", parentAgentDetails.Country),
                new SqlParameter("@County", parentAgentDetails.County),
                new SqlParameter("@State", parentAgentDetails.State),
                new SqlParameter("@ZipCode", parentAgentDetails.ZipCode),
                new SqlParameter("@Client_Id", parentAgentDetails.Client_Id),
                new SqlParameter("@Email", parentAgentDetails.Email),
                new SqlParameter("@FEIN", parentAgentDetails.FEIN)
            ];
            SqlParameter[] spParams = value;
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateParentAgentDetails", spParams);
            parentAgentDetails.ParentAgentDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@ParentAgencyDetails_Id").FirstOrDefault().Value);
            return parentAgentDetails;
        }
        public List<EIPost_Req> GetAllTransactionDetailsByDirections(long Quote_Id, long Client_Id, string Type)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Quote_Id", Quote_Id),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@Type", Type)
            };
            return SQLHelper.GetObjectList(new EIPost_Req(), "USP_Accounting_GetAllTransactionDetailsByDirections", sqlParameter);
        }
        public ReferralUnderwriter InsertUpdateReferralUnderwriter(SqlTransaction sqlTransaction, ReferralUnderwriter referralUnderwriter)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName="@ReferralUnderwriter_Id", Value = referralUnderwriter.ReferralUnderwriter_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Name", referralUnderwriter.Name),
                new SqlParameter("@Client_Id", referralUnderwriter.Client_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateReferralUnderwriter", sqlParameters);
            referralUnderwriter.ReferralUnderwriter_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@ReferralUnderwriter_Id").FirstOrDefault().Value);
            return referralUnderwriter;
        }
        public ReferralUnderwriter GetReferralUnderwriterDetailsByClientId(long ReferralUnderwriter_Id, long Client_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@ReferralUnderwriter_Id",ReferralUnderwriter_Id),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObject(new ReferralUnderwriter(), "USP_Accounting_GetReferralUnderwriterByClientId", sqlParameters);
        }
        public AgentBrokerContactDetails InsertUpdateAgentBrokerContactDetails(SqlTransaction sqlTransaction, AgentBrokerContactDetails agentBrokerContactDetails, long CreatedOrUpdatedBy)
        {
            SqlParameter[] spParams = {
                new SqlParameter(){ ParameterName = "@AgentBrokerContactDetail_Id", Value = agentBrokerContactDetails.AgentBrokerContactDetail_Id, Direction = ParameterDirection.InputOutput },
                new SqlParameter("@AgentBrokerContactReference_Id", agentBrokerContactDetails.AgentBrokerContactReference_Id),
                new SqlParameter("@Name", agentBrokerContactDetails.Name),
                new SqlParameter("@Client_Id", agentBrokerContactDetails.Client_Id),
                new SqlParameter("@Email", agentBrokerContactDetails.Email),
                new SqlParameter("@CreatedOrUpdatedBy", Convert.ToString(CreatedOrUpdatedBy))
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Acconting_InsertUpdateAgentBrokerContactDetails", spParams);
            agentBrokerContactDetails.AgentBrokerContactDetail_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@AgentBrokerContactDetail_Id").FirstOrDefault().Value);
            return agentBrokerContactDetails;
        }
        public string ProcessBillQueue(long Client_Id, long report_Id, byte fileType_Id, byte direction_Id, string selectedInvoicesJson, string billDetailJson, long user_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@Report_Id", report_Id),
                new SqlParameter("@FileType_Id", fileType_Id),
                new SqlParameter("@Direction_Id", direction_Id),
                new SqlParameter("@SelectedInvoicesJson", selectedInvoicesJson),
                new SqlParameter("@BillDetailJson", billDetailJson),
                new SqlParameter("@CreatedBy", Convert.ToString(user_Id))
            };
            return Convert.ToString(SQLHelper.ExecuteScalar("USP_Accounting_ProcessBillQueue", sqlParameters)) ?? string.Empty;
        }

        public List<InstallmentDetails> GetInstallmentDetailsByPolicy(long PolicyDetails_Id, long Client_Id, int InstallmentNo)
        {
            SqlParameter[] sqlParameter =
           {
                new SqlParameter("@PolicyDetails_Id", PolicyDetails_Id),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@CurrentInstallmentNo", InstallmentNo)
            };
            return SQLHelper.GetObjectList(new InstallmentDetails(), "USP_Accounting_GetRemainingInstallmentDetailsByPolicy", sqlParameter);
        }

        public string UpdateBillProcessQueueStatus(long Client_Id, long billProcess_IdOrPayment_Id, byte billProcessStatus_Id, long user_Id)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@BillProcess_IdOrPayment_Id", billProcess_IdOrPayment_Id),
                new SqlParameter("@BillProcessStatus_Id", billProcessStatus_Id),
                new SqlParameter("@UpdatedBy", user_Id),
            };
            return Convert.ToString(SQLHelper.ExecuteScalar("USP_Accounting_UpdateBillProcessQueueStatus", sqlParameters)) ?? string.Empty;
        }
        public List<LookupEntity> GetAllMortgageeDetailsByName(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllMortgageeDetailsByName", sqlParameter);
        }
    }
}
