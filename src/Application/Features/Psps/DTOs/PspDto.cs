// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Application.Features.Psps.DTOs
{
    [DebuggerDisplay("{Name}")]
    public class PspDto : IMapFrom<Psp>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }

        // Data comes via CSV file uploads
        public bool IsCsvBased { get; set; }
    }
}
