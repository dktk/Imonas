using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Imonas.Infrastructure.Persistence.Configurations
{
    public class KeyConfiguration : IEntityTypeConfiguration<KeyValue>
    {
        public void Configure(EntityTypeBuilder<KeyValue> builder)
        {
            builder.Ignore(e => e.DomainEvents);
        }
    }
}
