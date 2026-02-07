using Domain.Common;
using Domain.Entities;

namespace Domain.Events
{
    public class InvoiceCreatedEvent : DomainEvent
    {
        public InvoiceCreatedEvent(Invoice item)
        {
            Item = item;
        }

        public Invoice Item { get; }
    }
}
