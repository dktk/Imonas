using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace System
{
    public static class SafeWrapper
    {
        public static void ThrowIfDefaul<T>(T value, string valueName)
            where T : class
        {
            if (default(T) == value)
            {
                throw new ApplicationException($"{valueName} is null");
            }
        }

        public static async Task<TResult> SafeExecution<TResult>(Func<Task<TResult>> execution, ILogger logger, string pathName)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                logger.LogInformation("Starting: " + pathName);

                return await execution();
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured while executing {pathName}. {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                watch.Stop();

                logger.LogInformation($"End: {pathName} ({watch.ElapsedMilliseconds}ms)");
            }

            return default(TResult);
        }
    }
}
