// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.MedalionData.Gold;

namespace Domain.Entities
{
    public class ReconciliationComment : AuditableEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public PspSettlement Reconciliation { get; set; }
        public int ReconciliationId { get; set; }
    }
}
