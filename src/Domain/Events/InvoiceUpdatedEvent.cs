using Domain.Common;
using Domain.Entities;

namespace Domain.Events
{
    public class InvoiceUpdatedEvent : DomainEvent
    {
        public InvoiceUpdatedEvent(Invoice item)
        {
            Item = item;
        }

        public Invoice Item { get; }
    }
}
