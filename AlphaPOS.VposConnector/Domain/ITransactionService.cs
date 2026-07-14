using System;

namespace AlphaPOS.VposConnector.Domain
{
    public interface ITransactionService
    {
        string TestConnection();
        string StartTransaction(string jsonRequest);
        string PollStatus(string transactionId);
        string GetVoucher(string transactionId);
        string CancelTransaction(string transactionId);
        string ExecuteMetodo(string jsonRequest);
        string ExecuteCards(string jsonRequest);
        string ExecuteLysto(string jsonRequest);
        string TerminateService();
        void SetApiKey(string apiKey, string header = "X-Api-Key");
        void SetTimeout(int ms);
    }
}
