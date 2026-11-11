using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.External.CoverPay;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class CoverPayRepository : ICoverPayRepository
    {
        public CoverPayInvoiceDetails GetCoverPayInvoiceDetails(long clientId, long policyDetailId)
        {
            var coverPayInvoiceDetails = new CoverPayInvoiceDetails();

            try
            {
                string query = "USP_Accounting_GetAllOpenInvoicesByPolicyId";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@PolicyDetail_Id", policyDetailId)
                };

                using (SqlDataReader reader = SQLHelper.ExecuteReader(CommandType.StoredProcedure, query, parameters))
                {
                    #region Invoice Schedules

                    while (reader.Read())
                    {
                        coverPayInvoiceDetails.InvoiceSchedules.Add(new InvoiceSchedule
                        {
                            Invoice_Id = Convert.ToInt32(reader["Invoice_Id"]),
                            InstallmentNo = Convert.ToInt32(reader["InstallmentNo"]),
                            Type = Convert.ToString(reader["Type"]) ?? string.Empty,
                            DueDate = Convert.ToDateTime(reader["DueDate"]),
                            ChargeTiming = reader["ChargeTiming"] == DBNull.Value ? string.Empty : Convert.ToString(reader["ChargeTiming"]),
                            Installments = reader["Installments"] == DBNull.Value ? null : Convert.ToInt32(reader["Installments"])
                        });
                    }

                    #endregion

                    #region Invoice Amounts

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            coverPayInvoiceDetails.InvoiceAmounts.Add(new InvoiceAmount
                            {
                                Invoice_Id = Convert.ToInt32(reader["Invoice_Id"]),
                                Name = Convert.ToString(reader["Name"]) ?? string.Empty,
                                Description = Convert.ToString(reader["Description"]) ?? string.Empty,
                                Amount = Convert.ToDouble(reader["Amount"]),
                                TotalAmount = Convert.ToDouble(reader["TotalAmount"])
                            });
                        }
                    }

                    #endregion

                    #region Fees & Taxes

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            coverPayInvoiceDetails.InvoiceFeeTaxes.Add(new InvoiceFeeTax
                            {
                                Invoice_Id = Convert.ToInt32(reader["Invoice_Id"]),
                                Type = Convert.ToString(reader["Type"]) ?? string.Empty,
                                Name = Convert.ToString(reader["Name"]) ?? string.Empty,
                                Amount = Convert.ToDouble(reader["Amount"])
                            });
                        }
                    }

                    #endregion

                    #region Summary

                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            coverPayInvoiceDetails.Summary = new InvoiceSummary
                            {
                                TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                                InvoiceNo = Convert.ToString(reader["InvoiceNo"]) ?? string.Empty,
                                BillingType = Convert.ToString(reader["BillingType"]) ?? string.Empty,
                                Email = Convert.ToString(reader["Email"]) ?? string.Empty,
                                PhoneNumber = Convert.ToString(reader["PhoneNumber"]) ?? string.Empty,
                                FirstName = Convert.ToString(reader["FirstName"]) ?? string.Empty,
                                LastName = Convert.ToString(reader["LastName"]) ?? string.Empty,
                                AddressType = Convert.ToString(reader["AddressType"]) ?? string.Empty,
                                AddressLine1 = Convert.ToString(reader["AddressLine1"]) ?? string.Empty,
                                City = Convert.ToString(reader["City"]) ?? string.Empty,
                                State = Convert.ToString(reader["State"]) ?? string.Empty,
                                PostalCode = Convert.ToString(reader["PostalCode"]) ?? string.Empty,
                                Country = Convert.ToString(reader["Country"]) ?? string.Empty
                            };
                        }
                    }

                    #endregion
                }

                return coverPayInvoiceDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long InsertUpdateExternalPaymentDetails(ExternalPaymentDetails externalPaymentDetails, long user_Id)
        {
            try
            {
                const string query = "USP_Accounting_InsertUpdateExternalPaymentDetails";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@CreatedOrUpdatedBy", Convert.ToString(user_Id)),
                    new SqlParameter("@Client_Id", externalPaymentDetails.Client_Id),
                    new SqlParameter("@Service_Id", externalPaymentDetails.Service_Id),
                    new SqlParameter("@ExternalReferenceNo", string.IsNullOrWhiteSpace(externalPaymentDetails.ExternalReferenceNo) ? (object)DBNull.Value : externalPaymentDetails.ExternalReferenceNo),
                    new SqlParameter("@PolicyDetail_Id", externalPaymentDetails.PolicyDetail_Id),
                    new SqlParameter("@PolicyNumber", string.IsNullOrWhiteSpace(externalPaymentDetails.PolicyNumber) ? (object)DBNull.Value : externalPaymentDetails.PolicyNumber),
                    new SqlParameter("@PaymentLink", string.IsNullOrWhiteSpace(externalPaymentDetails.PaymentLink) ? (object)DBNull.Value : externalPaymentDetails.PaymentLink)
                };

                object result = SQLHelper.ExecuteScalar(CommandType.StoredProcedure, query, parameters);
                return Convert.ToInt64(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ExternalPaymentDetails GetExternalPaymentDetails(long clientId, int serviceId, long policyDetailId)
        {
            var externalPaymentDetails = new ExternalPaymentDetails();

            try
            {
                const string query = "USP_Accounting_GetExternalPaymentDetails";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@Service_Id", serviceId),
                    new SqlParameter("@PolicyDetail_Id", policyDetailId)
                };

                using (SqlDataReader reader = SQLHelper.ExecuteReader(CommandType.StoredProcedure, query, parameters))
                {
                    if (reader.Read())
                    {
                        externalPaymentDetails.ExternalPaymentDetail_Id = Convert.ToInt64(reader["ExternalPaymentDetail_Id"]);
                        externalPaymentDetails.Client_Id = Convert.ToInt64(reader["Client_Id"]);
                        externalPaymentDetails.Service_Id = Convert.ToInt32(reader["Service_Id"]);
                        externalPaymentDetails.ExternalReferenceNo = reader["ExternalReferenceNo"] == DBNull.Value ? string.Empty : Convert.ToString(reader["ExternalReferenceNo"]);
                        externalPaymentDetails.PolicyDetail_Id = reader["PolicyDetail_Id"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["PolicyDetail_Id"]);
                        externalPaymentDetails.PolicyNumber = reader["PolicyNumber"] == DBNull.Value ? string.Empty : Convert.ToString(reader["PolicyNumber"]);
                        externalPaymentDetails.PaymentLink = reader["PaymentLink"] == DBNull.Value ? string.Empty : Convert.ToString(reader["PaymentLink"]);
                    }
                }

                return externalPaymentDetails;
            }
            catch
            {
                throw;
            }
        }

        public List<CoverPayOutstandingPolicyDetails> GetOutstandingPolicyDetails(long Client_Id, long Entity_Id, byte EntityType_Id, string querySearch)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Entity_Id", Entity_Id),
                new SqlParameter("@EntityType_Id", EntityType_Id),
                new SqlParameter("@QuerySearch", querySearch),
            };
            return SQLHelper.GetObjectList(new CoverPayOutstandingPolicyDetails(), "USP_Accounting_GetOutstandingPolicyDetails_V2", sqlParameter);
        }

        public CoverPayOutstandingInvoiceDetails GetOutstandingInvoiceDetails(long clientId, string policyDetails_Id_Json)
        {
            var coverPayOutstandingInvoiceDetails = new CoverPayOutstandingInvoiceDetails();

            try
            {
                string query = "USP_Accounting_GetOutstandingInvoiceDetails";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@PolicyDetails_Id_Json", policyDetails_Id_Json)
                };

                using (SqlDataReader reader = SQLHelper.ExecuteReader(CommandType.StoredProcedure,query, parameters))
                {
                    #region Invoice Data
                    while (reader.Read())
                    {
                        coverPayOutstandingInvoiceDetails.InvoiceDetails.Add(new CoverPayOutstandingInvoiceDetailsData
                        {
                            PolicyDetails_Id = reader["PolicyDetails_Id"] != DBNull.Value ? Convert.ToInt64(reader["PolicyDetails_Id"]) : 0,
                            Invoice_Id = reader["Invoice_Id"] != DBNull.Value ? Convert.ToInt64(reader["Invoice_Id"]) : 0,
                            InvoiceNumber = reader["InvoiceNumber"] != DBNull.Value ? Convert.ToString(reader["InvoiceNumber"]) : string.Empty,
                            InvoiceDate = reader["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(reader["InvoiceDate"]) : DateTime.MinValue,
                            DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDouble(reader["Amount"]) : 0,
                            PaidAmount = reader["PaidAmount"] != DBNull.Value ? Convert.ToDouble(reader["PaidAmount"]) : 0,
                            OutstandingAmount = reader["OutstandingAmount"] != DBNull.Value ? Convert.ToDouble(reader["OutstandingAmount"]) : 0,
                            InstallmentNumber = reader["InstallmentNumber"] != DBNull.Value ? Convert.ToInt32(reader["InstallmentNumber"]) : 0,
                            Type = reader["Type"] != DBNull.Value ? Convert.ToString(reader["Type"]) : string.Empty
                        });
                    }

                    #endregion

                    #region Premium Data
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            coverPayOutstandingInvoiceDetails.PremiumDetails.Add(new CoverPayOutstandingInvoicePremiumDetails
                            {
                                PolicyDetails_Id = reader["PolicyDetails_Id"] != DBNull.Value ? Convert.ToInt64(reader["PolicyDetails_Id"]) : 0,
                                Invoice_Id = reader["Invoice_Id"] != DBNull.Value ? Convert.ToInt64(reader["Invoice_Id"]) : 0,
                                Type = reader["Type"] != DBNull.Value ? Convert.ToString(reader["Type"]) : string.Empty,
                                Description = reader["Description"] != DBNull.Value ? Convert.ToString(reader["Description"]) : string.Empty,
                                OutstandingAmount = reader["OutstandingAmount"] != DBNull.Value ? Convert.ToDouble(reader["OutstandingAmount"]) : 0
                            });
                        }
                    }

                    #endregion

                    #region Tax/Fee Data

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            coverPayOutstandingInvoiceDetails.TaxFeeDetails.Add(new CoverPayOutstandingInvoiceTaxFeeDetails
                            {
                                PolicyDetails_Id = reader["PolicyDetails_Id"] != DBNull.Value ? Convert.ToInt64(reader["PolicyDetails_Id"]) : 0,
                                Invoice_Id = reader["Invoice_Id"] != DBNull.Value ? Convert.ToInt64(reader["Invoice_Id"]) : 0,
                                Type = reader["Type"] != DBNull.Value ? Convert.ToString(reader["Type"]) : string.Empty,
                                Name = reader["Name"] != DBNull.Value ? Convert.ToString(reader["Name"]) : string.Empty,
                                OutstandingAmount = reader["OutstandingAmount"] != DBNull.Value ? Convert.ToDouble(reader["OutstandingAmount"]) : 0
                            });
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return coverPayOutstandingInvoiceDetails;
        }
    }
}
