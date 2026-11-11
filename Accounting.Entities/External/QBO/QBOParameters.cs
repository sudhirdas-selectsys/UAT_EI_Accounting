namespace Accounting.Entities.External.QBO
{
    public class QBOParameters
    {
        public string BaseURL { get; set; }
        public string Company_Id { get; set; }
        public string MinorVersion { get; set; }
        public string Client_Id { get; set; }
        public string Client_Secret_Id { get; set; }
        public string TokenEndpoint { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}
