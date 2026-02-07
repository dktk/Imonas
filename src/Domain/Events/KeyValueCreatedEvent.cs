
using Domain.Common;
using Domain.Entities;

namespace Domain.Events
{
    public class KeyValueCreatedEvent : DomainEvent
    {
        public KeyValueCreatedEvent(KeyValue item)
        {
            Item = item;
        }

        public KeyValue Item { get; }
    }
}
