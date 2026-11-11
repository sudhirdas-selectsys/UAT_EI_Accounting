namespace Accounting.Entities.DB.Transactions
{
    public class Clients : BaseEntity
    {
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string Address { get; set; }
        public string City {  get; set; }
        public string State {  get; set; }
        public string ZipCode {  get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[] Logo { get; set; }
        public byte AccountingService_Id { get; set; }
        public byte AccountingType_Id { get; set; }
        public byte PaymentService_Id { get; set; }
    }
}
