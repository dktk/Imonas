namespace Domain
{
    public interface IMethodService
    {
        Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to);
        Task<TargetData[]> GetTransactionsAsync(byte[] content);
    }

    public interface ISourceService
    {
        Task<InternalTransactionsResult> GetDataByTransactions(string[] transactionIds);
        string ServiceName { get; }
    }
}
