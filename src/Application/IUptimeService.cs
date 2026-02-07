namespace Application
{
    public interface IUptimeService
    {
        TimeSpan GetUptime(IDateTime dateTime);
        string GetUptimeString(IDateTime dateTime);
    }
}
