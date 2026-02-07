using SG.Common;

namespace Application
{
    public class ResultSafeWrapper(ILogger<ResultSafeWrapper> logger)
    {
        public async Task<Result<TReturn>> ExecuteAsync<TReturn, TRequest>(Func<Task<Result<TReturn>>> action, TRequest request, string errorMessage)
        {
            try
            {
                var result = await action();

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, request.ToJson());

                return Result<TReturn>.BuildFailure(errorMessage);
            }
        }
    }
}
