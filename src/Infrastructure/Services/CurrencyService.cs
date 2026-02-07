// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CurrencyService(IApplicationDbContext context) : ICurrencyService
    {
        // todo: use IMemoryCache here 
        private static List<Currency> currencies = new List<Currency>();
        private static readonly Currency NullCurrency = new Currency();

        public Currency GetByCode(string code)
        {
            var currency = currencies.FirstOrDefault(x => x.Code == code);

            if (currency != null)
            {
                return currency;
            }

            NullCurrency.Code = code;
            NullCurrency.HtmlCode = code;

            return NullCurrency;
        }

        public async Task Initialize()
        {
            currencies = await context.Currencies.ToListAsync();
        }
    }
}
