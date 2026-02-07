namespace Imonas.Exchange.Contracts
{
    public record class QueuePspData(DateTime startDate, DateTime endDate, int pspId, string externalSource, Guid correlationId);
}
