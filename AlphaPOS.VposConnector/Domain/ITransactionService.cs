namespace AlphaPOS.VposConnector.Domain
{
    public interface ITransactionService
    {
        string LastJsonRequest { get; }
        string LastJsonResponse { get; }
        string LastUrlRequest { get; }

        string TestConnection();
        string ExecuteMetodo(string jsonRequest);
        void SetBaseUrl(string baseUrl);
        void SetTimeout(int ms);
    }
}
