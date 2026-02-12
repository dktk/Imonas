using Application;
using Infrastructure.Persistence;

namespace Infrastructure.Services
{
    public enum SerilogLevel
    {
        Information,
        Warning,
        Error
    }

    public class SerilogsService(ApplicationDbContext context) : ISerilogsService
    {
        private readonly string _correlationId = Guid.NewGuid().ToString();

        private static readonly Dictionary<SerilogLevel, string> levels = new Dictionary<SerilogLevel, string>
        {
            { SerilogLevel.Information, nameof(SerilogLevel.Information) },
            { SerilogLevel.Warning, nameof(SerilogLevel.Warning) },
            { SerilogLevel.Error, nameof(SerilogLevel.Error) }
        };

        public async Task AddInfo(string message, string userName) => await Add(message, SerilogLevel.Information, userName);
        public async Task AddWarning(string message, string userName) => await Add(message, SerilogLevel.Warning, userName);
        public async Task AddError(string message, string userName, Exception exception = null) => await Add(message, SerilogLevel.Error, userName, exception);

        private async Task Add(string message, SerilogLevel level, string userName, Exception exception = null)
        {
            context.Serilogs.Add(new Domain.Entities.Log.Serilog()
            {
                Message = message,
                Level = levels[level],
                UserName = userName,
                TimeStamp = DateTime.UtcNow,
                Exception = exception?.Message + Environment.NewLine + exception?.StackTrace,
                CorrelationId = _correlationId
            });

            await context.SaveChangesAsync();
        }
    }
}
