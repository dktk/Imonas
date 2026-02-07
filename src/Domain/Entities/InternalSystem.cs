// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain.Entities
{
    public class InternalSystem : AuditableEntity
    {
        public string Name { get; set; }
        public ICollection<Psp> Psps { get; set; }
        public bool IsActive { get; set; }
    }
}
