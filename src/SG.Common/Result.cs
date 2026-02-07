namespace SG.Common
{
    public interface IResult
    {
        public bool Success { get; set; }
        public bool Failure { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public string[] Warnings { get; set; }
        public string ExtraInfo { get; set; }

        public List<string> MessageProperties { get; set; }
    }

    public class Result<T> : IResult
    {
        public T? Value { get; set; }
        public bool Success { get; set; }
        public bool Failed => !Success;
        public bool Failure { get; set; }

        private string _message;
        public string Message
        {
            set { _message = value; }
            get
            {
                if (!string.IsNullOrEmpty(_message))
                {
                    return _message;
                }

                if (Errors?.Length > 0)
                {
                    return string.Join(",", Errors);
                }

                return _message;
            }
        }


        public string[] Errors { get; set; }
        public string[] Warnings { get; set; }
        public string ExtraInfo { get; set; }

        public List<string> MessageProperties { get; set; }


        public static Result<T> BuildFailure(string message, params string[] properties)
        {
            return new Result<T>
            {
                Message = message,
                Success = false,
                MessageProperties = properties.ToList()
            };
        }

        /// When life beats naming conventions
        public Result<TProjection> BuildFailureFromFailure<TProjection>()
        {
            return new Result<TProjection>
            {
                Success = false,
                Message = Message,
                MessageProperties = MessageProperties,
                Errors = Errors
            };
        }

        public static Result<T> BuildSuccess(T value, string message = null)
        {
            return new Result<T>
            {
                Value = value,
                Message = message,
                Success = true
            };
        }

        public Result<TProjection> Map<TProjection>(Func<T, TProjection> map)
        {
            if (Success)
            {
                return Result<TProjection>.BuildSuccess(map(Value));
            }

            return BuildFailure<TProjection>();
        }

        public async Task<Result<TProjection>> MapAsync<TProjection>(Func<T, Task<TProjection>> map)
        {
            if (Success)
            {
                return Result<TProjection>.BuildSuccess(await map(Value));
            }

            return BuildFailure<TProjection>();
        }

        public async Task<Result<TProjection>> BindAsync<TProjection>(Func<T, Task<Result<TProjection>>> map)
        {
            if (Success)
            {
                return await map(Value);
            }

            return BuildFailure<TProjection>();
        }

        public async Task<Result<(TProjection1, TProjection2)>> BindAsync2<TProjection1, TProjection2>(
                    Func<T, Task<Result<TProjection1>>> map1,
                    Func<T, Task<Result<TProjection2>>> map2)
        {
            if (Failure)
            {
                return Result<(TProjection1, TProjection2)>.BuildFailure(Message);
            }

            var result1 = await map1(Value);

            if (result1.Failure)
            {
                return Result<(TProjection1, TProjection2)>.BuildFailure(result1.Message);
            }

            var result2 = await map2(Value);

            if (result2.Failure)
            {
                return Result<(TProjection1, TProjection2)>.BuildFailure(result2.Message);
            }

            var result = (result1.Value, result2.Value);

            return Result<(TProjection1, TProjection2)>.BuildSuccess(result);
        }

        public Result<TProjection> Bind<TProjection>(Func<T, TProjection> map)
        {
            return Failure
                ? BuildFailure<TProjection>()
                : Result<TProjection>.BuildSuccess(map(Value), Message);
        }

        public async Task<Result<TProjection>> Map<TProjection>(Func<T, Task<Result<TProjection>>> mapper)
        {
            if (Failure)
            {
                return BuildFailure<TProjection>();
            }

            var result = await mapper(Value);

            return result;
        }

        private Result<TProjection> BuildFailure<TProjection>()
        {
            var failure = Result<TProjection>.BuildFailure(Message);
            failure.Message = Message;
            failure.MessageProperties = MessageProperties;

            return failure;
        }

        public static new Result<T> CreateFailure(string errors, string extraInfo = null, string message = null)
        {
            return new Result<T> { Success = false, Errors = [errors], ExtraInfo = extraInfo };
        }

        public static new Result<T> CreateFailure(IEnumerable<string> errors, string extraInfo = null, string message = null)
        {
            return new Result<T> { Success = false, Errors = errors.ToArray(), ExtraInfo = extraInfo };
        }

        public static Result<T> CreateSuccess<T>(T data, string message = null)
        {
            return new Result<T> { Success = true, Value = data, Message = message };
        }

        public static Result<T> CreateWarning<T>(T data, IEnumerable<string> warnings, string message = null)
        {
            return new Result<T> { Success = false, Value = data, Warnings = warnings.ToArray(), Message = message };
        }
    }

    public static class ResultExtensions
    {
        public async static Task<Result<TProjection>> Map2<TInput, TProjection>(this Result<TInput> input, Func<TInput, Task<Result<TProjection>>> action)
        {
            if (input.Failure)
            {
                return Result<TProjection>.BuildFailure(input.Message);
            }

            return await action(input.Value);
        }

        public async static Task<Result<TProjection>> MapAsync2<TInput, TProjection>(this Task<Result<TInput>> inputTask, Func<Result<TInput>, Task<Result<TProjection>>> action)
        {
            var input = await inputTask;

            if (input.Failure)
            {
                return Result<TProjection>.BuildFailure(input.Message);
            }

            return await action(input);
        }
    }
}
