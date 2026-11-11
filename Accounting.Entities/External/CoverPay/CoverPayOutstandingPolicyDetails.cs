using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayOutstandingPolicyDetails
    {
        public string Currenty { get; set; } = "USD";
        public long PolicyDetails_Id { get; set; }
        public string BillType { get; set; }
        public string Payer_ExternalReference_Id { get; set; }
        public string Payer_Name { get; set; }
        public string Payer_Email { get; set; }
        public string Customer_ExternalReference_Id { get; set; }
        public string Customer_FirstName { get; set; }
        public string Customer_LastName { get; set; }
        public string Customer_Email { get; set; }
        public string Customer_Phone { get; set; }
        public string Customer_AddressType { get; set; }
        public string Customer_Adress1 { get; set; }
        public string Customer_Adress2 { get; set; }
        public string Customer_City { get; set; }
        public string Customer_State { get; set; }
        public string Customer_PostalCode { get; set; }
        public string Customer_Country { get; set; }
        public string Policy_ExternalReference_Id { get; set; }
        public string Policy_PolicyNumber { get; set; }
        public string Policy_InsuredName { get; set; }
        public string Policy_PolicyType { get; set; }
        public string Policy_LineOfBusiness_Code { get; set; }
        public string Policy_LineOfBusiness_Name { get; set; }
        public DateTime Policy_EffectiveDate { get; set; }
        public DateTime Policy_ExpirationDate { get; set; }
        public string Contacts_Agency_ContactType { get; set; }
        public string Contacts_Agency_ExternalReference_Id { get; set; }
        public string Contacts_Agency_CompanyName { get; set; }
        public string Contacts_Agency_NAIC { get; set; }
        public string Contacts_Agency_Addresses_Adress1 { get; set; }
        public string Contacts_Agency_Addresses_Adress2 { get; set; }
        public string Contacts_Agency_Addresses_City { get; set; }
        public string Contacts_Agency_Addresses_State { get; set; }
        public string Contacts_Agency_Addresses_PostalCode { get; set; }
        public string Contacts_Agency_Addresses_Country { get; set; }
        public string Contacts_Insured_ContactType { get; set; }
        public string Contacts_Insured_ExternalReference_Id { get; set; }
        public string Contacts_Insured_CompanyName { get; set; }
        public string Contacts_Insured_NAIC { get; set; }
        public string Contacts_Insured_Addresses_Adress1 { get; set; }
        public string Contacts_Insured_Addresses_Adress2 { get; set; }
        public string Contacts_Insured_Addresses_City { get; set; }
        public string Contacts_Insured_Addresses_State { get; set; }
        public string Contacts_Insured_Addresses_PostalCode { get; set; }
        public string Contacts_Insured_Addresses_Country { get; set; }
        public string External_AccountNumber { get; set; }
        public string External_PostalCode { get; set; }
    }
}
