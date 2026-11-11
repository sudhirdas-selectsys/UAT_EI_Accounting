namespace Accounting.Entities.Internal
{
    public class ServiceResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public bool IsTrustedDevice { get; set; }
        public bool IsMFA_Enable { get; set; }
    }
}
