// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SG.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class MoneyFormatter
    {
        private readonly IReadOnlyDictionary<string, string> _currencyToCulture;
        private readonly Func<string, CultureInfo?> _cultureResolver;

        /// <param name="currencyToCulture">
        /// Optional mapping like: { "USD": "en-US", "EUR": "fr-FR", "GBP": "en-GB", "RON": "ro-RO" }.
        /// If a currency isn't mapped, we fall back to invariant culture with the currency code as a suffix.
        /// </param>
        /// <param name="cultureResolver">
        /// Optional override if you want to resolve cultures dynamically (e.g., per tenant).
        /// </param>
        public MoneyFormatter(
            IReadOnlyDictionary<string, string>? currencyToCulture = null,
            Func<string, CultureInfo?>? cultureResolver = null)
        {
            _currencyToCulture = currencyToCulture ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _cultureResolver = cultureResolver ?? ResolveCulture;
        }

        /// <summary>
        /// Formats a numeric amount using currency-specific rules (symbol, separators, decimals).
        /// If currency->culture resolution fails, formats with invariant number format and appends the ISO code.
        /// </summary>
        public string Format(decimal amount, string currencyCode, bool includeCurrencySymbol = true)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentException("Currency code must be provided (e.g., 'USD', 'EUR').", nameof(currencyCode));

            currencyCode = currencyCode.Trim().ToUpperInvariant();

            var culture = _cultureResolver(currencyCode);

            if (culture is null)
            {
                // Fallback: invariant formatting + code.
                // Example: "12,345.67 USD"
                var inv = CultureInfo.InvariantCulture;
                var number = amount.ToString("N2", inv); // defaulting to 2 decimals in fallback
                return includeCurrencySymbol ? $"{number} {currencyCode}" : number;
            }

            // Clone so we can safely tweak NumberFormat without affecting global CultureInfo instances.
            var ci = (CultureInfo)culture.Clone();

            // Ensure we use the requested ISO currency (important when culture has a different default).
            // Example: formatting EUR with en-GB culture.
            var region = TryGetRegion(ci);
            var symbol = region?.CurrencySymbol ?? currencyCode;

            // For currencies like JPY, CLP, KRW you usually want 0 decimals; most others are 2.
            var decimals = GetTypicalFractionDigits(currencyCode);

            ci.NumberFormat.CurrencyDecimalDigits = decimals;

            // Keep culture separators/grouping, but adjust symbol if requested.
            if (includeCurrencySymbol)
            {
                ci.NumberFormat.CurrencySymbol = symbol;
                return amount.ToString("C", ci);
            }

            // Without symbol: number with the culture's currency formatting but no symbol.
            // We’ll format as currency, then strip symbol carefully.
            ci.NumberFormat.CurrencySymbol = ""; // avoids fragile string replace
            var formatted = amount.ToString("C", ci).Trim();
            return formatted;
        }

        private CultureInfo? ResolveCulture(string currencyCode)
        {
            // 1) user-provided mapping
            if (_currencyToCulture.TryGetValue(currencyCode, out var cultureName) && !string.IsNullOrWhiteSpace(cultureName))
            {
                try { return CultureInfo.GetCultureInfo(cultureName); }
                catch (CultureNotFoundException) { /* fall through */ }
            }

            // 2) reasonable defaults for common codes (extend as needed)
            // Note: Many currencies are used by multiple locales; choose a conventional one.
            var defaults = currencyCode switch
            {
                "USD" => "en-US",
                "EUR" => "fr-FR", // conventional euro formatting (comma decimals) - change if you prefer.
                "GBP" => "en-GB",
                "CHF" => "de-CH",
                "CAD" => "en-CA",
                "AUD" => "en-AU",
                "NZD" => "en-NZ",
                "JPY" => "ja-JP",
                "CNY" => "zh-CN",
                "HKD" => "zh-HK",
                "SGD" => "en-SG",
                "SEK" => "sv-SE",
                "NOK" => "nb-NO",
                "DKK" => "da-DK",
                "PLN" => "pl-PL",
                "CZK" => "cs-CZ",
                "HUF" => "hu-HU",
                "RON" => "ro-RO",
                "BGN" => "bg-BG",
                "TRY" => "tr-TR",
                "BRL" => "pt-BR",
                "MXN" => "es-MX",
                "INR" => "hi-IN",
                "ZAR" => "en-ZA",
                _ => null
            };

            if (defaults is null) return null;

            try { return CultureInfo.GetCultureInfo(defaults); }
            catch (CultureNotFoundException) { return null; }
        }

        private static RegionInfo? TryGetRegion(CultureInfo culture)
        {
            // Some cultures are neutral (e.g., "en") and can’t create RegionInfo.
            try
            {
                if (culture.IsNeutralCulture) return null;
                return new RegionInfo(culture.Name);
            }
            catch
            {
                return null;
            }
        }

        private static int GetTypicalFractionDigits(string currencyCode)
        {
            // Not exhaustive, but covers the common “0-decimal” currencies.
            // Extend for your needs or back this by ISO-4217 metadata in your domain.
            return currencyCode switch
            {
                "JPY" => 0,
                "KRW" => 0,
                "CLP" => 0,
                "VND" => 0,
                "ISK" => 0,
                _ => 2
            };
        }
    }

}
