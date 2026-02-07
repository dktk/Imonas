// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application.Features.Psps.DTOs
{
    public class ReconciliationDataDto
    {   
        public required int PspId { get; set; }

        // todo:
        public required string ExternalSystem { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required int ReconciliationRunId { get; set; }
    }
}
