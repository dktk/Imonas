using Domain.Common;
using Domain.Entities;

namespace Domain.Events
{
    public class InvoiceUploadedEvent : DomainEvent
    {
        public InvoiceUploadedEvent(Invoice item)
        {
            Item = item;
        }

        public Invoice Item { get; }
    }
}
