// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;

using SG.Common;

namespace Infrastructure.Extensions;

public static class IdentityResultExtensions
{
    public static Result<ApplicationUser> ToApplicationResult(this ApplicationUser result)
    {
        return Result<ApplicationUser>.CreateSuccess(result);
    }

    public static Result<IdentityResult> ToApplicationResult(this IdentityResult result)
    {
        return Result<IdentityResult>.CreateSuccess(result);
    }
}
