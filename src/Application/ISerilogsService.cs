// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application
{
    public interface ISerilogsService
    {
        Task AddInfo(string message, string userName);
        Task AddWarning(string message, string userName);
        Task AddError(string message, string userName, Exception exception = null);
    }
}
