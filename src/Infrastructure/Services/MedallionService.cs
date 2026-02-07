// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class BronzeMedallionService(IApplicationDbContext applicationDbContext)
    {
        public async Task Store<T>()
        {
            //applicationDbContext.Payments.
        }
    }
}
