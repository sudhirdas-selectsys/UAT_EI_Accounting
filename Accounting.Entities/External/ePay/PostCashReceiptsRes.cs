namespace Accounting.Entities.External.ePay
{
    public class PostCashReceiptsRes
    {
        public PostCashReceiptsRes() 
        {
            Errors = new List<PostCashReceiptsErrors>();
        }
        public bool Success { get; set; }
        public List<PostCashReceiptsErrors> Errors { get; set; }
    }
    public class PostCashReceiptsErrors
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ErrorStackTrace { get; set; }
        public string ErrorMessage { get; set; }
    }
}
