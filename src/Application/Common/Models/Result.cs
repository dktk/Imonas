//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.

//namespace Application.Common.Models;

//public class Result : IResult
//{
//    internal Result()
//    {

//    }
//    internal Result(bool succeeded, IEnumerable<string> errors, string extraInfo = null)
//    {
//        Succeeded = succeeded;
//        Errors = errors.ToArray();
//        ExtraInfo = extraInfo;
//    }

//    public bool Succeeded { get; set; }
//    public bool Failed => !Succeeded;

//    public string[] Errors { get; set; }
//    public string[] Warnings { get; set; }
//    public string ExtraInfo { get; set; }

//    public static Result Success()
//    {
//        return new Result(true, Array.Empty<string>());
//    }

//    public static Task<Result<bool>> SuccessAsync(string extraInfo = null)
//    {
//        return Task.FromResult(new Result(true, Array.Empty<string>(), extraInfo));
//    }

//    public static Result Failure(IEnumerable<string> errors, string extraInfo = null)
//    {
//        return new Result(false, errors);
//    }

//    public static Task<Result<bool>> FailureAsync(IEnumerable<string> errors)
//    {
//        return Task.FromResult(new Result(false, errors));
//    }
//}
//public class Result<T> : Result, IResult<T>
//{
//    public T Data { get; set; }
//    public string Message { get; set; }

//    public static new Result<T> Failure(IEnumerable<string> errors, string extraInfo = null, string message = null)
//    {
//        return new Result<T> { Succeeded = false, Errors = errors.ToArray(), ExtraInfo = extraInfo };
//    }
//    public static new async Task<Result<T>> FailureAsync(IEnumerable<string> errors, string message = null)
//    {

//        return await Task.FromResult(Failure(errors, message: message));
//    }
//    public static Result<T> Success(T data, string message = null)
//    {
//        return new Result<T> { Succeeded = true, Data = data, Message = message };
//    }
//    public static Result<T> Warning(T data, IEnumerable<string> warnings, string message = null)
//    {
//        return new Result<T> { Succeeded = false, Data = data, Warnings = warnings.ToArray(), Message = message };
//    }
//    public static async Task<Result<T>> WarningAsync(T data, IEnumerable<string> warnings, string message = null)
//    {
//        return await Task.FromResult(Warning(data, warnings, message));
//    }
//    public static async Task<Result<T>> SuccessAsync(T data, string message = null)
//    {
//        return await Task.FromResult(Success(data, message));
//    }
//}

//public static class ResultExtensions
//{
//    public static Result<TResult> Success<TResult>(this TResult result, string extraInfo = null, string message = null, string[] warnings = null)
//    {
//        return new Result<TResult>
//        {
//            Data = result,
//            Succeeded = true,
//            ExtraInfo = extraInfo,
//            Message = message,
//            Warnings = warnings
//        };
//    }
//}
