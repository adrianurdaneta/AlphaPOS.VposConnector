namespace AlphaPOS.VposConnector.Domain
{
    public interface ITransactionService
    {
        string TestConnection();
        string ExecuteMetodo(string jsonRequest);
        void SetBaseUrl(string baseUrl);
        void SetTimeout(int ms);
    }
}
