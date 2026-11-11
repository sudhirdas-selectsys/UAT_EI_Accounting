namespace Accounting.Entities.DB.Transactions
{
    public class ServiceLogInfo : BaseEntity
    {
        public long ServiceLogInfo_Id { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public bool Is_Success { get; set; }
        public string Message { get; set; }
        public int ResponseTime_In_Sec { get; set; }
        public byte Service_Id { get; set; }
        public byte ServiceRequestType_Id { get; set; }
    }
}
