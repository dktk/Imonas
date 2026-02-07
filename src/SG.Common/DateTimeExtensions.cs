namespace System
{
    public static class DateTimeExtensions
    {
        public static string PrettyDateTime(this DateTime d) => d.ToString("yyyy-MM-dd HH:mm:ss");
        
        //todo: find out what kind of ISO this is
        public static string MongoDateTime(this DateTime d) => d.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        public static string CompressedPrettyDateTime(this DateTime d) => d.ToString("yyyyMMdd_HHmmss");
    }
}
