using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;

        public DateTime Today => DateTime.Today;
        public DateTime Yesterday => Today.AddDays(-1);
        public DateTime Tomorrow => Today.AddDays(1);
        public DateTime TwoDaysBack => Today.AddDays(-2);
    }
}
