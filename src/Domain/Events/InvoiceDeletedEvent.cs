using Domain.Common;
using Domain.Entities;

namespace Domain.Events
{
    public class InvoiceDeletedEvent : DomainEvent
    {
        public InvoiceDeletedEvent(Invoice item)
        {
            Item = item;
        }

        public Invoice Item { get; }
    }
}
