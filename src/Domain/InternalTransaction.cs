// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Domain
{
    /// <summary>
    /// Data from our internal systems. 
    /// </summary>
    [DebuggerDisplay("{RefNumber} - {ProviderTxId}")]
    public class InternalTransaction
    {
        public InternalTransaction() {}
        public string Id { get; set; }
        public required string RefNumber { get; set; }
        public required DateTimeOffset RequestDate { get; set; }
        public string Status { get; set; }
        public required decimal Amount { get; set; }
        public required string CurrencyCode { get; set; }
        public required string Email { get; set; }
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public required string ProviderTxId { get; set; }
        public required string System { get; set; }
    }

    /// <summary>
    /// A split between txs found vs not found in the Internal System.
    /// </summary>
    public class InternalTransactionsResult
    {
        public InternalTransaction[] Found { get; set; } = [];
        public List<string> NotFoundTransactionIds { get; set; } = [];
    }
}
